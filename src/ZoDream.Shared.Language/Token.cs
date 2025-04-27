namespace ZoDream.Shared.Language
{
    public struct Token(TokenType type, string value, CursorPosition begin, CursorPosition end)
    {
        public TokenType Type { get; private set; } = type;

        public string Value { get; private set; } = value;

        public CursorPosition Begin { get; private set; } = begin;

        public CursorPosition End { get; private set; } = end;

        public readonly int Length => (int)(End.Index - Begin.Index);

        public override readonly string ToString()
            => string.Format("[{0,4},{1,4} - {2,4},{3,4}] {4}='{5}'", Begin.Line, Begin.Column, End.Line, End.Column, Type, Value);

    }

    public struct CursorPosition(int line, int column, long index)
    {
        /// <summary>
        /// 行号，从1开始
        /// </summary>
        public int Line = line;
        /// <summary>
        /// 列，从1开始
        /// </summary>
        public int Column = column;
        /// <summary>在整个内容中的位置，从0开始</summary>
        public long Index = index;

        public override readonly string ToString()
            => string.Format("({0}; {1}; {2})", Line, Column, Index);
    }

    public enum TokenType : byte
    {
        /// <summary>Not defined token</summary>
        None,
        /// <summary>End of file</summary>
        Eof,

        /// <summary>Invalid char</summary>
        InvalidChar,
        /// <summary>Invalid string</summary>
        InvalidString,
        /// <summary>Invalid string opening</summary>
        InvalidStringOpening,
        /// <summary>Invalid comment</summary>
        InvalidComment,

        NewLine,
        Whitespace,
        Comment,
        CommentBlock,
        /// <summary>
        /// null
        /// </summary>
        Nil,
        True,
        False,
        String,
        Number,
        /// <summary>Identifier</summary>
        Identifier,
        /// <summary>
        /// 定义数据类型
        /// </summary>
        DataType,
        /// <summary>
        /// 运算符 + - * / = 
        /// </summary>
        Operator,
        /// <summary>
        /// 复合运算符 ++ -- += -= *= /= ^= ~=
        /// </summary>
        CompoundOperator,
        /// <summary>
        /// 分隔符 ,;
        /// </summary>
        Separator,
        /// <summary>
        /// 判断符 == != && ||
        /// </summary>
        Determiner,
        /// <summary>
        /// 内部保留关键词
        /// </summary>
        ReservedKeywords,


        /// <summary>
        /// 成对出现的括号 []
        /// </summary>
        BracketOpen,
        /// <summary>
        /// ]
        /// </summary>
        BracketClose,
        /// <summary>
        /// (
        /// </summary>
        ParenOpen,
        /// <summary>
        /// )
        /// </summary>
        ParenClose,
        /// <summary>
        /// {
        /// </summary>
        BraceOpen,
        /// <summary>
        /// }
        /// </summary>
        BraceClose,
        /// <summary>
        /// \ |
        /// </summary>
        Backslash,
        /// <summary>
        /// :
        /// </summary>
        Colon,
        /// <summary>
        /// ,
        /// </summary>
        Comma,
        /// <summary>
        /// .
        /// </summary>
        Dot,
        /// <summary>
        /// ;
        /// </summary>
        Semicolon,
        /// <summary>
        /// ?
        /// </summary>
        Optional,
        /// <summary>
        /// @
        /// </summary>
        At,
        /// <summary>
        /// =
        /// </summary>
        Equal,
        /// <summary>
        ///  <
        /// </summary>
        LessThan,
        /// <summary>
        /// >
        /// </summary>
        GreaterThan,
        /// <summary>
        /// &&
        /// </summary>
        And,
        /// <summary>
        /// ||
        /// </summary>
        Or,
        /// <summary>
        /// !
        /// </summary>
        Not,
     
    }
}
