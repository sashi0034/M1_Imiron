#nullable enable

namespace M1_Imiron;

public record struct TokenPosition(
    int Line,
    int Character
);

public record struct Token(
    string Text,
    int Line,
    int Character
);