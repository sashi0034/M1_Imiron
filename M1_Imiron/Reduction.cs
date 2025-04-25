namespace M1_Imiron;

public static class Reduction
{
    public static string Reduce(string content)
    {
        var tokenList = Tokenizer.Tokenize(content);

        var rule = Parser.ExpectRule(new ParserState(tokenList));

        var innerList = new List<string>();
        for (var reductionIndex = 0; reductionIndex < rule.Reduction.Length; reductionIndex++)
        {
            var reduction = rule.Reduction[reductionIndex];
            // Console.WriteLine($"Reducing [{reductionIndex}]: {rule.Content} ==> {reduction}");

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