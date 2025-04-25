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
            var matches = Regex.Matches(lineText, @"\w+|[()\[\]{};]");
            foreach (Match match in matches)
            {
                bool isAlphabet = Regex.IsMatch(match.Value, @"\w+");
                var token = match.Value;
                var character = match.Index;

                tokenList.Add(new Token(
                    isAlphabet ? TokenKind.Alphabet : TokenKind.Mark,
                    token,
                    curretntLine,
                    character
                ));
            }

            curretntLine++;
        }

        return tokenList;
    }
}