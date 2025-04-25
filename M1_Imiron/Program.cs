namespace M1_Imiron;

internal static class Program
{
    public static void Main()
    {
        var input = Console.ReadLine();
        var result = Reduction.Reduce(input);

        Console.WriteLine("\n----------------------------------------------- Result");

        Console.WriteLine(result);

        var testCode =
            "\n----------------------------------------------- Generate the test\n" +
            "assertEqual(\n" +
            "\"\"\"\n" +
            result + "\n" +
            "\"\"\", Reduction.Reduce(\"" + input + "\"));\n";
        Console.WriteLine(testCode);
    }
}