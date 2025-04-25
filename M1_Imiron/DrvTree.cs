namespace M1_Imiron;

public interface IDrvTree
{
    public string Content { get; }
}

// DrvZ ::= 'Z'
public record DrvZ(
) : IDrvTree
{
    public string Content => "Z";
}

// DrvS ::= 'S' '(' DrvParam ')'
public record DrvS(
    DrvParam Inner
) : IDrvTree
{
    public string Content => $"S({Inner.Content})";
}

// DrvParam ::= DrvZ | DrvS
public record DrvParam(
    IDrvTree Param
) : IDrvTree
{
    public string Content => Param.Content;
}

public interface IDrvRule : IDrvTree
{
    public string[] Reduction { get; }
}

// DrvPZero ::= DrvZ plus DrvParam is DrvParam
public record DrvPZero(
    DrvParam N
) : IDrvRule
{
    public string Content => $"Z plus {N.Content} is {N.Content} by P-Zero";

    public string[] Reduction => [];
}

// DrvPSucc ::= DrvS plus DrvParam is DrvS
public record DrvPSucc(
    DrvS N1,
    DrvParam N2,
    DrvS N
) : IDrvRule
{
    public string Content =>
        $"{N1.Content} plus {N2.Content} is {N.Content} by P-Succ";

    public string[] Reduction => [$"{N1.Inner.Content} plus {N2.Param.Content} is {N.Inner.Content}"];
}

// DrvTZero ::= DrvZ times TrvParameter is DrvZ
public record DrvTZero(
    DrvParam N
) : IDrvRule
{
    public string Content => $"Z times {N.Content} is Z by T-Zero";

    public string[] Reduction => [];
}

// DrvTSucc ::= DrvS times DrvParam is DrvParam
public record DrvTSucc(
    DrvS N1,
    DrvParam N2,
    string N3,
    DrvParam N4
) : IDrvRule
{
    public string Content => $"{N1.Content} times {N2.Content} is {N4.Content} by T-Succ";

    public string[] Reduction =>
    [
        $"{N1.Inner.Content} times {N2.Param.Content} is {N3}",
        $"{N2.Content} plus {N3} is {N4.Content}"
    ];
}