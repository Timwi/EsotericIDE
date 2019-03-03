using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic
{
    public enum ParseErrorType
    {
        NONE,
        INVALID_CHARACTER,
        NO_ENTRY,
        NO_TERMINATOR,
        INVALID_MODIFIER,
        TOO_MANY_MODIFIERS
    }

    public struct ParseError
    {
        public ParseErrorType type;
        public Vector2Int pos;
        public int pos1d;
        public char character;

        public ParseError(ParseErrorType type, Vector2Int pos, int pos1, char character)
        {
            this.type = type;
            this.pos = pos;
            pos1d = pos1;
            this.character = character;
        }
    }
}
