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
    public static IDrvRule ExpectRule(ParserState parser)
    {
        var lhs = parseValue(parser);
        if (lhs == null) throw abort(parser);

        var op = parser.NextOpt()?.Text;
        if (op != "plus" && op != "times") throw abort(parser);
        parser.Step();

        var rhs = parseValue(parser);
        if (rhs == null) throw abort(parser);

        if (parser.NextOpt()?.Text != "is") throw abort(parser);
        parser.Step();

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
}