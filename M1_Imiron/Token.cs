namespace M1_Imiron;

public record struct TokenPosition(
    int Line,
    int Character
);

public enum TokenKind
{
    Alphabet,
    Mark
}

public record Token(
    TokenKind Kind,
    string Text,
    int Line,
    int Character
);