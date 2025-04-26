namespace M1_Imiron;

public interface IDrvExpr
{
    public string Content { get; }
}

// DrvValue ::= DrvZ | DrvS
public interface IDrvValue : IDrvExpr
{
    public int Value { get; }
}

// DrvZ ::= 'Z'
public record DrvZ(
) : IDrvValue
{
    public string Content => "Z";

    public int Value => 0;
}

// DrvS ::= 'S' '(' DrvValue ')'
public record DrvS(
    IDrvValue Inner
) : IDrvValue
{
    public string Content => $"S({Inner.Content})";

    public int Value => 1 + Inner.Value;

    public static IDrvValue FromValue(int value)
    {
        if (value <= 0) return new DrvZ();
        return new DrvS(FromValue(value - 1));
    }
}

public interface IDrvRule : IDrvExpr
{
    public string[] Reduction { get; }
}

// DrvPZero ::= DrvZ plus DrvValue is DrvValue
public record DrvPZero(
    IDrvValue N
) : IDrvRule
{
    public string Content => $"Z plus {N.Content} is {N.Content} by P-Zero";

    public string[] Reduction => [];
}

// DrvPSucc ::= DrvS plus DrvValue is DrvS
public record DrvPSucc(
    DrvS N1,
    IDrvValue N2,
    DrvS N
) : IDrvRule
{
    public string Content =>
        $"{N1.Content} plus {N2.Content} is {N.Content} by P-Succ";

    public string[] Reduction => [$"{N1.Inner.Content} plus {N2.Content} is {N.Inner.Content}"];
}

// DrvTZero ::= DrvZ times TrvParameter is DrvZ
public record DrvTZero(
    IDrvValue N
) : IDrvRule
{
    public string Content => $"Z times {N.Content} is Z by T-Zero";

    public string[] Reduction => [];
}

// DrvTSucc ::= DrvS times DrvValue is DrvValue
public record DrvTSucc(
    DrvS N1,
    IDrvValue N2,
    IDrvValue N4
) : IDrvRule
{
    public readonly IDrvValue N3 = DrvS.FromValue(N1.Inner.Value * N2.Value);

    public string Content => $"{N1.Content} times {N2.Content} is {N4.Content} by T-Succ";

    public string[] Reduction =>
    [
        $"{N1.Inner.Content} times {N2.Content} is {N3.Content}",
        $"{N2.Content} plus {N3.Content} is {N4.Content}"
    ];
}

// DrvLSucc ::= DrvValue is less than DrvS
public record DrvLSucc(
    IDrvValue N
) : IDrvRule
{
    public string Content => $"{N.Content} is less than S({N.Content}) by L-Succ";

    public string[] Reduction => [];
}

// DrvLTrans ::= DrvValue is less than DrvValue
public record DrvLTrans(
    IDrvValue N1,
    IDrvValue N3
) : IDrvRule
{
    public readonly IDrvValue N2 = DrvS.FromValue(N1.Value - 1);

    public string Content => $"{N1.Content} is less than {N3.Content} by L-Trans";

    public string[] Reduction =>
    [
        $"{N1.Content} is less than {N2.Content}",
        $"{N2.Content} is less than {N3.Content}"
    ];
}