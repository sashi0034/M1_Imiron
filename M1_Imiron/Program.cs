namespace M1_Imiron;

internal static class Program
{
    public static void Main()
    {
        var input = "S(S(Z)) is less than S(S(S(S(S(Z)))))";
        var tokenList = Tokenizer.Tokenize(input);
        foreach (var token in tokenList)
        {
            Console.WriteLine($"Token: {token.Text}, Line: {token.Line}, Character: {token.Character}");
        }
    }
}