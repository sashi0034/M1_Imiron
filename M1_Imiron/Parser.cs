using System.Diagnostics;

namespace M1_Imiron;

public class ParserState
{
    private readonly List<Token> _tokens;
    private int _parseIndex = 0;

    public ParserState(List<Token> tokens, int parseIndex = 0)
    {
        _tokens = tokens;
        _parseIndex = parseIndex;
    }

    public Token Next() => _tokens[_parseIndex];

    public Token? NextOpt() => IsEnd() ? null : _tokens[_parseIndex];

    public void Step() => _parseIndex++;

    public bool IsEnd() => _parseIndex >= _tokens.Count;

    public ParserState Clone() => new(_tokens, _parseIndex);
}

public static class Parser
{
    private static void expectToken(ParserState parser, string text)
    {
        if (parser.IsEnd())
        {
            throw new Exception("Unexpected end of input");
        }

        if (parser.Next().Text != text)
        {
            throw new Exception(
                $"Expected '{text}' but found '{parser.Next().Text}' at {parser.Next().Line}:{parser.Next().Character}");
        }

        parser.Step();
    }

    private static Exception abort(ParserState parser)
    {
        if (parser.IsEnd())
        {
            return new Exception("Unexpected end of input");
        }

        return new Exception(
            $"Unexpected token '{parser.Next().Text}' at {parser.Next().Line}:{parser.Next().Character}");
    }

    private static Exception fail(ParserState parser)
    {
        return new Exception("Failed to parse the rule");
    }

    // DrvZ ::= 'Z'
    private static DrvZ? parseZ(ParserState parser)
    {
        if (parser.NextOpt()?.Text != "Z") return null;
        parser.Step();
        return new DrvZ();
    }

    // DrvS ::= 'S' '(' DrvValue ')'
    private static DrvS? parseS(ParserState parser)
    {
        if (parser.NextOpt()?.Text != "S") return null;
        parser.Step();

        expectToken(parser, "(");

        IDrvNat? parameter = parseNat(parser);
        if (parameter == null) throw abort(parser);

        expectToken(parser, ")");

        return new DrvS(parameter);
    }

    // DrvValue ::= DrvZ | DrvS
    private static IDrvNat? parseNat(ParserState parser)
    {
        var nat = parseZ(parser) ?? (IDrvNat?)parseS(parser);
        return nat;
    }

    // DrvPZero ::= DrvZ plus DrvValue is DrvValue
    // DrvPSucc ::= DrvS plus DrvValue is DrvS
    // DrvTZero ::= DrvZ times TrvParameter is DrvZ
    // DrvTSucc ::= DrvS times DrvValue is DrvValue
    private static IDrvRule? parseNatRule(ParserState parser)
    {
        var lhs = parseNat(parser);
        if (lhs == null) return null;

        var op = parser.NextOpt()?.Text;
        if (op != "plus" && op != "times") return null;
        parser.Step();

        var rhs = parseNat(parser);
        if (rhs == null) throw abort(parser);

        expectToken(parser, "is");

        var result = parseNat(parser);
        if (result == null) throw abort(parser);

        if (op == "plus")
        {
            if (lhs.Content == "Z" && rhs.Content == result.Content)
            {
                return new DrvPZero(rhs);
            }
            else if (lhs is DrvS lhsS && result is DrvS resultS)
            {
                return new DrvPSucc(lhsS, rhs, resultS);
            }
        }
        else
        {
            Debug.Assert(op == "times");
            if (lhs.Content == "Z" && result.Content == "Z")
            {
                return new DrvTZero(rhs);
            }
            else if (lhs is DrvS lhsS)
            {
                return new DrvTSucc(lhsS, rhs, result);
            }
        }

        throw fail(parser);
    }

    // -----------------------------------------------

    // lhs is less than rhs
    private static bool parseLhsIsLessThanRhs(ParserState parser, out IDrvNat? lhs, out IDrvNat? rhs)
    {
        lhs = parseNat(parser);
        if (lhs == null)
        {
            rhs = null;
            return true;
        }

        if (parser.NextOpt()?.Text != "is")
        {
            rhs = null;
            return true;
        }

        parser.Step();

        if (parser.NextOpt()?.Text != "less")
        {
            rhs = null;
            return true;
        }

        parser.Step();

        expectToken(parser, "than");

        rhs = parseNat(parser);
        if (rhs == null) throw abort(parser);
        return false;
    }

    // DrvLSucc ::= DrvValue is less than DrvS
    // DrvLTrans ::= DrvValue is less than DrvValue
    public static IDrvRule? parseCompareNat1Rule(ParserState parser)
    {
        if (parseLhsIsLessThanRhs(parser, out var lhs, out var rhs)) return null;
        Debug.Assert(lhs != null && rhs != null);

        if (lhs.Value == rhs.Value - 1)
        {
            return new DrvLSucc(lhs);
        }
        else
        {
            return new DrvLTrans(lhs, rhs);
        }
    }

    // DrvLZero ::= DrvZ is less than DrvS
    // DrvLSuccSucc ::= DrvS is less than DrvS
    public static IDrvRule? parseCompareNat2Rule(ParserState parser)
    {
        if (parseLhsIsLessThanRhs(parser, out var lhs, out var rhs)) return null;
        Debug.Assert(lhs != null && rhs != null);

        if (lhs.Value == 0)
        {
            return new DrvLZero(rhs);
        }
        else if (lhs is DrvS lhsS && rhs is DrvS rhsS)
        {
            return new DrvLSuccSucc(lhsS, rhsS);
        }

        throw fail(parser);
    }

    // DrvLSucc ::= DrvValue is less than DrvS
    // DrvLSuccR :== DrvValue is less than DrvS
    public static IDrvRule? parseCompareNat3Rule(ParserState parser)
    {
        if (parseLhsIsLessThanRhs(parser, out var lhs, out var rhs)) return null;
        Debug.Assert(lhs != null && rhs != null);

        if (lhs.Value == rhs.Value - 1)
        {
            return new DrvLSucc(lhs);
        }
        else if (rhs is DrvS rhsS)
        {
            return new DrvLSuccR(lhs, rhsS);
        }

        throw fail(parser);
    }

    public static IDrvRule? parseCompareNatRule(ParserState parser)
    {
        string directive = "";
        if (parser.NextOpt()?.Kind == TokenKind.Directive)
        {
            directive = parser.Next().Text;
            parser.Step();
        }

        if (directive == "#CompareNat1")
        {
            return parseCompareNat1Rule(parser);
        }
        else if (directive == "#CompareNat2")
        {
            return parseCompareNat2Rule(parser);
        }
        else if (directive == "#CompareNat3")
        {
            return parseCompareNat3Rule(parser);
        }
        else
        {
            return parseCompareNat1Rule(parser.Clone()) ??
                   parseCompareNat2Rule(parser.Clone()) ??
                   parseCompareNat3Rule(parser.Clone());
        }
    }

    // -----------------------------------------------

    // DrcFactor :== '(' DrcExpr ')' | DrcNat
    private static DrvFactor? parseFactor(ParserState parser)
    {
        if (parser.NextOpt()?.Text == "(")
        {
            parser.Step();

            var inner = parseExpr(parser);
            if (inner == null) throw abort(parser);

            expectToken(parser, ")");
            return new DrvFactor(inner);
        }

        var nat = parseNat(parser);
        return nat != null ? new DrvFactor(nat) : null;
    }

    // DrcTerm :== DrvNat {* DrvNat}
    private static DrvTerm? parseTerm(ParserState parser)
    {
        var head = parseFactor(parser);
        if (head == null) return null;

        List<DrvFactor> tail = [];

        while (true)
        {
            if (parser.NextOpt()?.Text != "*") break;
            parser.Step();

            var rhs = parseFactor(parser);
            if (rhs == null) throw abort(parser);

            tail.Add(rhs);
        }

        return new DrvTerm(head, tail);
    }

    // DrvExpr ::= DrvTerm {+ DrcTerm}
    private static DrvExpr? parseExpr(ParserState parser)
    {
        var head = parseTerm(parser);
        if (head == null) return null;

        List<DrvTerm> tail = [];

        while (true)
        {
            if (parser.NextOpt()?.Text != "+") break;
            parser.Step();

            var rhs = parseTerm(parser);
            if (rhs == null) throw abort(parser);

            tail.Add(rhs);
        }

        return new DrvExpr(head, tail);
    }

    private static IDrvExpRule? parseExpRule(ParserState parser)
    {
        var expr = parseExpr(parser);
        if (expr == null) return null;

        if (parser.NextOpt()?.Text != "evalto") return null;
        parser.Step();

        var nat = parseNat(parser);
        if (nat == null) throw abort(parser);

        return parseExprRule_internal(expr, nat);
    }

    private static IDrvExpRule? parseExprRule_internal(DrvExpr expr, IDrvNat nat)
    {
        if (expr.Tail.Count > 0)
        {
            return new DrvEPlus(expr.PopBack(), expr.Tail[^1], nat);
        }

        var term = expr.Head;
        if (term.Tail.Count > 0)
        {
            return new DrvETimes(term.PopBack(), term.Tail[^1], nat);
        }

        if (term.Head.ExprOpt != null)
        {
            return parseExprRule_internal(term.Head.ExprOpt, nat);
        }

        return new DrvEConst(term.Head);
    }

    public static IDrvRule ExpectRule(ParserState parser)
    {
        var nat = parseNatRule(parser.Clone());
        if (nat != null) return nat;

        var compareNat = parseCompareNatRule(parser.Clone());
        if (compareNat != null) return compareNat;

        var exprRule = parseExpRule(parser.Clone());
        if (exprRule != null) return exprRule;

        throw fail(parser);
    }
}