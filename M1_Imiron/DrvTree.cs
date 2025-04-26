namespace M1_Imiron;

public interface IDrv
{
    public string Content { get; }
}

// DrvValue ::= DrvZ | DrvS
public interface IDrvNat : IDrv
{
    public int Value { get; }
}

// DrvZ ::= 'Z'
public record DrvZ(
) : IDrvNat
{
    public string Content => "Z";

    public int Value => 0;
}

// DrvS ::= 'S' '(' DrvValue ')'
public record DrvS(
    IDrvNat Inner
) : IDrvNat
{
    public string Content => $"S({Inner.Content})";

    public int Value => 1 + Inner.Value;

    public static IDrvNat FromValue(int value)
    {
        if (value <= 0) return new DrvZ();
        return new DrvS(FromValue(value - 1));
    }
}

public interface IDrvRule : IDrv
{
    public string[] Reduction { get; }
}

// DrvPZero ::= DrvZ plus DrvValue is DrvValue
public record DrvPZero(
    IDrvNat N
) : IDrvRule
{
    public string Content => $"Z plus {N.Content} is {N.Content} by P-Zero";

    public string[] Reduction => [];
}

// DrvPSucc ::= DrvS plus DrvValue is DrvS
public record DrvPSucc(
    DrvS N1,
    IDrvNat N2,
    DrvS N
) : IDrvRule
{
    public string Content =>
        $"{N1.Content} plus {N2.Content} is {N.Content} by P-Succ";

    public string[] Reduction => [$"{N1.Inner.Content} plus {N2.Content} is {N.Inner.Content}"];
}

// DrvTZero ::= DrvZ times TrvParameter is DrvZ
public record DrvTZero(
    IDrvNat N
) : IDrvRule
{
    public string Content => $"Z times {N.Content} is Z by T-Zero";

    public string[] Reduction => [];
}

// DrvTSucc ::= DrvS times DrvValue is DrvValue
public record DrvTSucc(
    DrvS N1,
    IDrvNat N2,
    IDrvNat N4
) : IDrvRule
{
    public readonly IDrvNat N3 = DrvS.FromValue(N1.Inner.Value * N2.Value);

    public string Content => $"{N1.Content} times {N2.Content} is {N4.Content} by T-Succ";

    public string[] Reduction =>
    [
        $"{N1.Inner.Content} times {N2.Content} is {N3.Content}",
        $"{N2.Content} plus {N3.Content} is {N4.Content}"
    ];
}

// DrvLSucc ::= DrvValue is less than DrvS
public record DrvLSucc(
    IDrvNat N
) : IDrvRule
{
    public string Content => $"{N.Content} is less than S({N.Content}) by L-Succ";

    public string[] Reduction => [];
}

// DrvLTrans ::= DrvValue is less than DrvValue
public record DrvLTrans(
    IDrvNat N1,
    IDrvNat N3
) : IDrvRule
{
    public readonly IDrvNat N2 = DrvS.FromValue(N1.Value + 1);

    public string Content => $"{N1.Content} is less than {N3.Content} by L-Trans";

    public string[] Reduction =>
    [
        $"{N1.Content} is less than {N2.Content}",
        $"{N2.Content} is less than {N3.Content}"
    ];
}

// DrvLZero ::= DrvZ is less than DrvS
public record DrvLZero(
    IDrvNat N
) : IDrvRule
{
    public string Content => $"Z is less than {N.Content} by L-Zero";

    public string[] Reduction => [];
}

// DrvLSuccSucc ::= DrvS is less than DrvS
public record DrvLSuccSucc(
    DrvS N1,
    DrvS N2
) : IDrvRule
{
    public string Content => $"{N1.Content} is less than {N2.Content} by L-SuccSucc" +
                             $"";

    public string[] Reduction => [$"{N1.Inner.Content} is less than {N2.Inner.Content}",];
}

// DrvLSuccR :== DrvValue is less than DrvS
public record DrvLSuccR(
    IDrvNat N1,
    DrvS N2
) : IDrvRule
{
    public string Content => $"{N1.Content} is less than {N2.Content} by L-SuccR";

    public string[] Reduction => [$"{N1.Content} is less than {N2.Inner.Content}"];
}

public interface IDrvExp : IDrvNat;

// DrcTerm :== DrvNat {* DrvNat}
public record DrvTerm(
    IDrvNat Head,
    List<IDrvNat> Tail
) : IDrvExp
{
    public IEnumerable<IDrvNat> Terms =>
        new[] { Head }.Concat(Tail);

    public string Content => string.Join(" * ", Terms.Select(t => t.Content));

    public int Value => Terms.Aggregate(1, (acc, t) => acc * t.Value);

    public DrvTerm PopBack()
    {
        return new DrvTerm(Head, Tail.Slice(0, Tail.Count - 1));
    }

    // public DrvTerm PopFront()
    // {
    //     return new DrvTerm(Tail[0], Tail.Slice(1, Tail.Count - 1));
    // }
}

// DrvExpr ::= DrvTerm {+ DrcTerm}
public record DrvExpr(
    DrvTerm Head,
    List<DrvTerm> Tail
) : IDrvExp
{
    public IEnumerable<DrvTerm> Terms =>
        new[] { Head }.Concat(Tail);

    public string Content => string.Join(" + ", Terms.Select(t => t.Content));

    public int Value => Terms.Sum(t => t.Value);

    public DrvExpr PopBack()
    {
        return new DrvExpr(Head, Tail.Slice(0, Tail.Count - 1));
    }

    // public DrvExpr PopFront()
    // {
    //     return new DrvExpr(Tail[0], Tail.Slice(1, Tail.Count - 1));
    // }
}

public interface IDrvExpRule : IDrvRule
{
    public int Value { get; }
}

// DrvEConst :== DrvValue evalto DrvValue
public record DrvEConst(
    IDrvNat N
) : IDrvExpRule
{
    public string Content => $"{N.Content} evalto {N.Content} by E-Const";

    public string[] Reduction => [];

    public int Value => N.Value;
}

// DrvEPlus :== DrvValue + DrvValue evalto DrvValue
public record DrvEPlus(
    IDrvExp E1,
    IDrvExp E2,
    IDrvNat N
) : IDrvExpRule
{
    public IDrvNat N1 => DrvS.FromValue(E1.Value);

    public IDrvNat N2 => DrvS.FromValue(E2.Value);

    public string Content => $"{E1.Content} + {E2.Content} evalto {N.Content} by E-Plus";

    public string[] Reduction =>
    [
        $"{E1.Content} evalto {N1.Content}",
        $"{E2.Content} evalto {N2.Content}",
        $"{N1.Content} plus {N2.Content} is {N.Content}"
    ];

    public int Value => N.Value;
}

// DrvETimes :== DrvValue * DrvValue evalto DrvValue
public record DrvETimes(
    IDrvNat E1,
    IDrvNat E2,
    IDrvNat N
) : IDrvExpRule
{
    public IDrvNat N1 => DrvS.FromValue(E1.Value);

    public IDrvNat N2 => DrvS.FromValue(E2.Value);

    public string Content => $"{E1.Content} * {E2.Content} evalto {N.Content} by E-Times";

    public string[] Reduction =>
    [
        $"{E1.Content} evalto {N1.Content}",
        $"{E2.Content} evalto {N2.Content}",
        $"{N1.Content} times {N2.Content} is {N.Content}"
    ];

    public int Value => N.Value;
}