using System.Text.RegularExpressions;

namespace M1_Imiron;

public static class Tokenizer
{
    public static List<Token> Tokenize(string text)
    {
        var lineTextList = text.Split('\n');

        int curretntLine = 0;

        var tokenList = new List<Token>();
        while (true)
        {
            if (curretntLine >= lineTextList.Length)
                break;

            var lineText = lineTextList[curretntLine];

            // アルファベット or '(', ']', '{', '}', ';'
            var matches = Regex.Matches(lineText, @"\w+|[()\[\]{};]|#\w+");
            foreach (Match match in matches)
            {
                var tokenText = match.Value;
                var character = match.Index;

                TokenKind kind = TokenKind.Mark;
                if (Regex.IsMatch(tokenText, @"\w+")) kind = TokenKind.Alphabet;
                if (tokenText.StartsWith("#")) kind = TokenKind.Directive;

                tokenList.Add(new Token(kind, tokenText, curretntLine, character));
            }

            curretntLine++;
        }

        return tokenList;
    }
}