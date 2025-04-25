// See https://aka.ms/new-console-template for more information

using M1_Imiron;
using Xunit;

namespace Test;

public static class Test
{
    private static void assertEqual(string expected, string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');

        Assert.Equal(expectedLines.Length, actualLines.Length);

        for (int i = 0; i < expectedLines.Length; i++)
        {
            Assert.Equal(expectedLines[i].Trim(), actualLines[i].Trim());
        }
    }


    [Fact]
    public static void Example_0()
    {
        assertEqual(
            """
            S(Z) times S(Z) is S(Z) by T-Succ {
               Z times S(Z) is Z by T-Zero {};
               S(Z) plus Z is S(Z) by P-Succ {
                   Z plus Z is Z by P-Zero {}
               }
            }
            """, Reduction.Reduce("S(Z) times S(Z) is S(Z)"));
    }
}