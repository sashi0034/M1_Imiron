namespace M1_Imiron;

internal static class Program
{
    public static void Main()
    {
        var input = "S(Z) times S(Z) is S(Z)";
        var result = Reduction.Reduce(input);
        Console.WriteLine(result);
    }
}