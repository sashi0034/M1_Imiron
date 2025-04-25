namespace M1_Imiron;

public static class Reduction
{
    public static string Reduce(string content)
    {
        var tokenList = Tokenizer.Tokenize(content);

        var rule = Parser.ExpectRule(new ParserState(tokenList));

        var innerList = new List<string>();
        foreach (var reduction in rule.Reduction)
        {
            // Console.WriteLine("Reducing: " + content + " ==> " + reduction);

            innerList.Add(addIndentToEachLine(Reduce(reduction)));
        }

        var br = innerList.Count > 0 ? "\n" : "";

        return rule.Content + " {" + br +
               string.Join(";\n", innerList) + br +
               "}";
    }

    private static string addIndentToEachLine(string text, string indent = "    ")
    {
        return string.Join("\n", text
            .Split('\n')
            .Select(line => indent + line));
    }
}