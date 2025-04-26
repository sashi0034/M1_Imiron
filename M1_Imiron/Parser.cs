using System.Diagnostics;

namespace M1_Imiron;

public class ParserState
{
    private readonly List<Token> _tokens;
    private int _parseIndex = 0;

    public ParserState(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public Token Next() => _tokens[_parseIndex];

    public Token? NextOpt() => IsEnd() ? null : _tokens[_parseIndex];

    public void Step() => _parseIndex++;

    public bool IsEnd() => _parseIndex >= _tokens.Count;

    public ParserState Clone() => new(_tokens);
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

        IDrvValue? parameter = parseValue(parser);
        if (parameter == null) throw abort(parser);

        expectToken(parser, ")");

        return new DrvS(parameter);
    }


    // DrvValue ::= DrvZ | DrvS
    private static IDrvValue? parseValue(ParserState parser)
    {
        var value = parseZ(parser) ?? (IDrvValue?)parseS(parser);
        return value;
    }

    // DrvPZero ::= DrvZ plus DrvValue is DrvValue
    // DrvPSucc ::= DrvS plus DrvValue is DrvS
    // DrvTZero ::= DrvZ times TrvParameter is DrvZ
    // DrvTSucc ::= DrvS times DrvValue is DrvValue
    private static IDrvRule? parsePlusOrTimesRule(ParserState parser)
    {
        var lhs = parseValue(parser);
        if (lhs == null) return null;

        var op = parser.NextOpt()?.Text;
        if (op != "plus" && op != "times") return null;
        parser.Step();

        var rhs = parseValue(parser);
        if (rhs == null) throw abort(parser);

        expectToken(parser, "is");

        var result = parseValue(parser);
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

    // DrvLSucc ::= DrvValue is less than DrvS
    // DrvLTrans ::= DrvValue is less than DrvValue
    public static IDrvRule? parseLessThanRule(ParserState parser)
    {
        var lhs = parseValue(parser);
        if (lhs == null) return null;

        if (parser.NextOpt()?.Text != "is") return null;
        parser.Step();

        if (parser.NextOpt()?.Text != "less") return null;
        parser.Step();

        expectToken(parser, "than");

        var rhs = parseValue(parser);
        if (rhs == null) throw abort(parser);

        if (lhs.Value == rhs.Value - 1)
        {
            return new DrvLSucc(lhs);
        }
        else
        {
            return new DrvLTrans(lhs, rhs);
        }
    }

    public static IDrvRule ExpectRule(ParserState parser)
    {
        var plusOrMinus = parsePlusOrTimesRule(parser.Clone());
        if (plusOrMinus != null) return plusOrMinus;

        var lessThan = parseLessThanRule(parser.Clone());
        if (lessThan != null) return lessThan;

        throw fail(parser);
    }
}