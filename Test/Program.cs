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

    [Fact]
    public static void Nat()
    {
        assertEqual(
            "Z plus Z is Z by P-Zero {}", Reduction.Reduce("Z plus Z is Z"));

        assertEqual(
            """
            Z plus S(S(Z)) is S(S(Z)) by P-Zero {}
            """, Reduction.Reduce("Z plus S(S(Z)) is S(S(Z))\n"));

        assertEqual(
            """
            S(S(Z)) plus Z is S(S(Z)) by P-Succ {
                S(Z) plus Z is S(Z) by P-Succ {
                    Z plus Z is Z by P-Zero {}
                }
            }
            """, Reduction.Reduce("S(S(Z)) plus Z is S(S(Z))"));

        assertEqual(
            """
            S(Z) plus S(S(S(Z))) is S(S(S(S(Z)))) by P-Succ {
                Z plus S(S(S(Z))) is S(S(S(Z))) by P-Zero {}
            }
            """, Reduction.Reduce("S(Z) plus S(S(S(Z))) is S(S(S(S(Z))))"));

        assertEqual(
            """
            Z times S(S(Z)) is Z by T-Zero {}
            """, Reduction.Reduce("Z times S(S(Z)) is Z"));

        assertEqual(
            """
            S(S(Z)) times Z is Z by T-Succ {
                S(Z) times Z is Z by T-Succ {
                    Z times Z is Z by T-Zero {};
                    Z plus Z is Z by P-Zero {}
                };
                Z plus Z is Z by P-Zero {}
            }
            """, Reduction.Reduce("S(S(Z)) times Z is Z"));

        assertEqual(
            """
            S(S(Z)) times S(Z) is S(S(Z)) by T-Succ {
                S(Z) times S(Z) is S(Z) by T-Succ {
                    Z times S(Z) is Z by T-Zero {};
                    S(Z) plus Z is S(Z) by P-Succ {
                        Z plus Z is Z by P-Zero {}
                    }
                };
                S(Z) plus S(Z) is S(S(Z)) by P-Succ {
                    Z plus S(Z) is S(Z) by P-Zero {}
                }
            }
            """, Reduction.Reduce("S(S(Z)) times S(Z) is S(S(Z))"));

        assertEqual(
            """
            S(S(Z)) times S(S(Z)) is S(S(S(S(Z)))) by T-Succ {
                S(Z) times S(S(Z)) is S(S(Z)) by T-Succ {
                    Z times S(S(Z)) is Z by T-Zero {};
                    S(S(Z)) plus Z is S(S(Z)) by P-Succ {
                        S(Z) plus Z is S(Z) by P-Succ {
                            Z plus Z is Z by P-Zero {}
                        }
                    }
                };
                S(S(Z)) plus S(S(Z)) is S(S(S(S(Z)))) by P-Succ {
                    S(Z) plus S(S(Z)) is S(S(S(Z))) by P-Succ {
                        Z plus S(S(Z)) is S(S(Z)) by P-Zero {}
                    }
                }
            }
            """, Reduction.Reduce("S(S(Z)) times S(S(Z)) is S(S(S(S(Z))))"));
    }

    [Fact]
    public static void CompareNat()
    {
        assertEqual(
            """
            S(S(Z)) is less than S(S(S(Z))) by L-Succ {}
            """, Reduction.Reduce("#CompareNat1 S(S(Z)) is less than S(S(S(Z)))"));

        assertEqual(
            """
            S(S(Z)) is less than S(S(S(Z))) by L-SuccSucc {
                S(Z) is less than S(S(Z)) by L-SuccSucc {
                    Z is less than S(Z) by L-Zero {}
                }
            }
            """, Reduction.Reduce("#CompareNat2 S(S(Z)) is less than S(S(S(Z)))"));

        assertEqual(
            """
            S(S(Z)) is less than S(S(S(Z))) by L-Succ {}
            """, Reduction.Reduce("#CompareNat3 S(S(Z)) is less than S(S(S(Z)))"));

        assertEqual(
            """
            S(S(Z)) is less than S(S(S(S(S(Z))))) by L-Trans {
                S(S(Z)) is less than S(S(S(Z))) by L-Succ {};
                S(S(S(Z))) is less than S(S(S(S(S(Z))))) by L-Trans {
                    S(S(S(Z))) is less than S(S(S(S(Z)))) by L-Succ {};
                    S(S(S(S(Z)))) is less than S(S(S(S(S(Z))))) by L-Succ {}
                }
            }
            """, Reduction.Reduce("#CompareNat1 S(S(Z)) is less than S(S(S(S(S(Z)))))"));

        assertEqual(
            """
            S(S(Z)) is less than S(S(S(S(S(Z))))) by L-SuccSucc {
                S(Z) is less than S(S(S(S(Z)))) by L-SuccSucc {
                    Z is less than S(S(S(Z))) by L-Zero {}
                }
            }
            """, Reduction.Reduce("#CompareNat2 S(S(Z)) is less than S(S(S(S(S(Z)))))"));

        assertEqual(
            """
            S(S(Z)) is less than S(S(S(S(S(Z))))) by L-SuccR {
                S(S(Z)) is less than S(S(S(S(Z)))) by L-SuccR {
                    S(S(Z)) is less than S(S(S(Z))) by L-Succ {}
                }
            }
            """, Reduction.Reduce("#CompareNat3 S(S(Z)) is less than S(S(S(S(S(Z)))))"));
    }

    [Fact]
    public static void EvalNatExp()
    {
        assertEqual(
            """
            Z + S(S(Z)) evalto S(S(Z)) by E-Plus {
                Z evalto Z by E-Const {};
                S(S(Z)) evalto S(S(Z)) by E-Const {};
                Z plus S(S(Z)) is S(S(Z)) by P-Zero {}
            }
            """, Reduction.Reduce("Z + S(S(Z)) evalto S(S(Z))"));

        assertEqual(
            """
            S(S(Z)) + Z evalto S(S(Z)) by E-Plus {
                S(S(Z)) evalto S(S(Z)) by E-Const {};
                Z evalto Z by E-Const {};
                S(S(Z)) plus Z is S(S(Z)) by P-Succ {
                    S(Z) plus Z is S(Z) by P-Succ {
                        Z plus Z is Z by P-Zero {}
                    }
                }
            }
            """, Reduction.Reduce("S(S(Z)) + Z evalto S(S(Z))"));
    }
}