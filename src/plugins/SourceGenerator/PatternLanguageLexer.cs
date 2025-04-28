using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    /// <summary>
    /// ImHex Patterns 语言解析
    /// </summary>
    public class PatternLanguageLexer(TextReader reader) : ILexer
    {
        /// <summary>
        /// 读取多了需要，排个队列
        /// </summary>
        private readonly Queue<Token> NextTokenQueue = new();
        /// <summary>
        /// 上一次获取到的Token
        /// </summary>
        public Token Current {  get; private set; }
        private int _lineIndex = 0;
        private int _columnIndex = 0;
        private int _charIndex = -1;
        // 上一个字符
        private int _lastChar = -1;
        // 当前的字符
        private int _currentChar = -1;
        // 指示下一次只获取当前的
        private bool _moveNextStop = false;

        object IEnumerator.Current => Current;

        public Token NextToken()
        {
            Token token;
            if (NextTokenQueue.Count > 0)
            {
                token = NextTokenQueue.Dequeue();
            }
            else
            {
                token = ReadToken();
            }
            Current = token;
            return token;
        }

        public bool MoveNext()
        {
            if (Current.Type == TokenType.Eof)
            {
                return false;
            }
            NextToken();
            return true;
        }

        public void Reset()
        {
            if (reader is StreamReader o)
            {
                o.BaseStream.Position = 0;
            } else
            {
                // 不支持
            }
        }

        private Token ReadToken()
        {
            int next;
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt == -1)
                {
                    return CreateToken(TokenType.Eof, string.Empty, 0, 0);
                }
                if (IsNewLine(codeInt))
                {
                    return CreateToken(TokenType.NewLine, 0, 0);
                }
                var code = (char)codeInt;
                if (IsWhiteSpace(code))
                {
                    continue;
                }
                if (code is '\'' or '"')
                {
                    return GetStringToken(code);
                }
                if (code is >= '0' and <= '9')
                {
                    return GetNumericToken(code);
                }
                if (code is ',')
                {
                    return CreateToken(TokenType.Comma, code.ToString(), 0, 1);
                }
                if (code is ';')
                {
                    return CreateToken(TokenType.Semicolon, code.ToString(), 0, 1);
                }
                if (code is '[')
                {
                    next = ReadChar();
                    if (next == '[')
                    {
                        return CreateToken(TokenType.BracketOpen, "[[", 1, 1);
                    }
                    MoveBackChar();
                    return CreateToken(TokenType.BracketOpen, code.ToString(), 0, 1);
                }
                if (code is ']')
                {
                    next = ReadChar();
                    if (next == ']')
                    {
                        return CreateToken(TokenType.BracketClose, "]]", 1, 1);
                    }
                    MoveBackChar();
                    return CreateToken(TokenType.BracketClose, code.ToString(), 0, 1);
                }
                if (code is '(')
                {
                    return CreateToken(TokenType.ParenOpen, code.ToString(), 0, 1);
                }
                if (code is ')')
                {
                    return CreateToken(TokenType.ParenClose, code.ToString(), 0, 1);
                }
                if (code is '{')
                {
                    return CreateToken(TokenType.BraceOpen, code.ToString(), 0, 1);
                }
                if (code is '}')
                {
                    return CreateToken(TokenType.BraceClose, code.ToString(), 0, 1);
                }
                if (code == '.')
                {
                    next = ReadChar();
                    if (next == '.')
                    {
                        return CreateToken(TokenType.CompoundOperator, "..", 1, 1);
                    }
                    MoveBackChar();
                    return CreateToken(TokenType.Dot, code.ToString(), 0, 1);
                }
                if (code == ':')
                {
                    next = ReadChar();
                    if (next == ':')
                    {
                        return CreateToken(TokenType.CompoundOperator, "::", 1, 1);
                    }
                    MoveBackChar();
                    return CreateToken(TokenType.Colon, code.ToString(), 0, 1);
                }
                if (code == '/')
                {
                    next = ReadChar();
                    if (next < 0)
                    {
                        return CreateToken(TokenType.InvalidChar, code.ToString(), 0, 0);
                    }
                    if (next == '=')
                    {
                        return CreateToken(TokenType.CompoundOperator, "/=", 2, 0);
                    }
                    if (next == '*')
                    {
                        return GetCommentBlockToken();
                    }
                    if (next == '/')
                    {
                        return GetCommentToken();
                    }
                    MoveBackChar();
                    continue;
                }
                if (code == '_')
                {
                    return GetNameToken(code);
                }
                if (code == '#')
                {
                    return GetPreprocessorToken(code);
                }
                if (code == '$')
                {
                    return CreateToken(TokenType.ReservedKeywords, code.ToString(), 0, 1);
                }
                if (code == '@')
                {
                    return CreateToken(TokenType.At, code.ToString(), 0, 1);
                }
                if (IsCombinationSymbol(code))
                {
                    return GetCombinationSymbol(code);
                }
                if (!IsWhiteSpace(code))
                {
                    return GetNameToken(code);
                }
            }
        }

        private Token GetNameToken(char? code)
        {
            var sb = new StringBuilder();
            if (code is not null)
            {
                sb.Append(code);
            }
            var begin = CreatePosition();
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                if (IsNewLine(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                var c = (char)codeInt;
                if (IsWhiteSpace(c))
                {
                    MoveBackChar();
                    break;
                }
                if (!(IsNumeric(codeInt) || IsAlphabet(codeInt)) || codeInt > 127)
                {
                    MoveBackChar();
                    break;
                }
                sb.Append(c);
            }
            return CreateToken(sb.ToString(), begin);
        }

        private Token GetCombinationSymbol(char code)
        {
            var sb = new StringBuilder();
            sb.Append(code);
            var begin = CreatePosition();
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                var c = (char)codeInt;
                if (IsWhiteSpace(c))
                {
                    break;
                }
                if (!IsCombinationSymbol(c))
                {
                    MoveBackChar();
                    break;
                }
                sb.Append(c);
            }
            var text = sb.ToString();
            if (text == "=>" || text == "=<")
            {
                text = string.Concat(text.AsSpan(1, 1), text.AsSpan(0, 1));
            }
            if (IsCompoundOperator(text))
            {
                return CreateToken(TokenType.CompoundOperator, text, begin);
            }
            if (IsOperator(text))
            {
                return new Token(TokenType.Operator, text, begin, CreatePosition());
            }
            switch (text)
            {
                case "//":
                    return GetCommentToken();
                case "/*":
                    return GetCommentBlockToken();
                case "!":
                    return new Token(TokenType.Not, text, begin, CreatePosition());
                case ">":
                    return new Token(TokenType.GreaterThan, text, begin, CreatePosition());
                case "<":
                    return new Token(TokenType.LessThan, text, begin, CreatePosition());
                case "=":
                    return new Token(TokenType.Equal, text, begin, CreatePosition());
                case "||":
                    return new Token(TokenType.Or, text, begin, CreatePosition());
                case "&&":
                    return new Token(TokenType.And, text, begin, CreatePosition());
                default:
                    break;
            }
            if (IsDeterminer(text))
            {
                return new Token(TokenType.Determiner, text, begin, CreatePosition());
            }
            return new Token(TokenType.InvalidChar, text, begin, CreatePosition());
        }



        /// <summary>
        /// 单行注释
        /// </summary>
        /// <returns></returns>
        private Token GetCommentToken()
        {
            var sb = new StringBuilder();
            var begin = CreatePosition();
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                if (IsNewLine(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                sb.Append((char)codeInt);
            }
            return new Token(TokenType.Comment, sb.ToString(), begin, CreatePosition());
        }
        /// <summary>
        /// 多行注释
        /// </summary>
        /// <returns></returns>
        private Token GetCommentBlockToken()
        {
            var sb = new StringBuilder();
            var begin = CreatePosition();
            var foundStar = false;
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                if (codeInt == '/' && foundStar)
                {
                    break;
                }
                if (foundStar)
                {
                    sb.Append('*');
                }
                if (codeInt == '*')
                {
                    foundStar = true;
                    continue;
                }
                foundStar = false;
                sb.Append((char)codeInt);
            }
            return new Token(TokenType.Comment, sb.ToString(), begin, CreatePosition());
        }


        private Token GetNumericToken(char code)
        {
            var isHex = 0; // 是否是十六进制, 0 未判断 1 是小数 2 是进制
            var sb = new StringBuilder();
            var begin = CreatePosition();
            sb.Append(code);
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                if (codeInt == '.')
                {
                    var next = ReadChar();
                    if (next == '.')
                    {
                        NextTokenQueue.Enqueue(CreateToken(TokenType.CompoundOperator, "..", 1, 1));
                        return CreateToken(TokenType.String, sb.ToString(), 2, -1);
                    }
                    isHex = 1;
                    sb.Append((char)codeInt);
                    if (next < 0)
                    {
                        break;
                    }
                    codeInt = next;
                }
                if (isHex == 0)
                {
                    if (codeInt != 'X' && codeInt != 'x' && !IsHexNumeric(codeInt))
                    {
                        MoveBackChar();
                        break;
                    }
                    if (!IsNumeric(codeInt))
                    {
                        isHex = 2;
                    }
                }
                else if (isHex == 1)
                {
                    if (!IsNumeric(codeInt))
                    {
                        MoveBackChar();
                        break;
                    }
                }
                else if (isHex == 2 && !IsHexNumeric(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                sb.Append((char)codeInt);
            }
            return new Token(TokenType.String, sb.ToString(), begin, CreatePosition());
        }

        private Token GetStringToken(char end)
        {
            var reverseCount = 0;
            var sb = new StringBuilder();
            var begin = CreatePosition();
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0)
                {
                    break;
                }
                if (codeInt == end && reverseCount % 2 == 0)
                {
                    break;
                }
                if (codeInt == '\\')
                {
                    reverseCount++;
                    if (reverseCount == 2)
                    {
                        sb.Append((char)codeInt);
                        reverseCount = 0;
                    }
                    continue;
                }
                reverseCount = 0;
                sb.Append((char)codeInt);
            }
            return new Token(TokenType.String, sb.ToString(), begin, CreatePosition());
        }

        private Token GetPreprocessorToken(char code)
        {
            var token = GetNameToken(code);
            GetIndent();
            if (token.Value is "#define" or "#pragma")
            {
                NextTokenQueue.Enqueue(GetNameToken(null));
                GetIndent();
            }
            var begin = CreatePosition();
            NextTokenQueue.Enqueue(CreateToken(GetLineRemaining(), begin));
            return token;
        }

        private string GetLineRemaining()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0 || IsNewLine(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                sb.Append((char)codeInt);
            }
            return sb.ToString();
        }

        private int GetIndent()
        {
            var count = 0;
            while (true)
            {
                var codeInt = ReadChar();
                if (codeInt < 0 || IsNewLine(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                if (codeInt == '\t')
                {
                    count += 4;
                    continue;
                }
                if (!IsWhiteSpace(codeInt))
                {
                    MoveBackChar();
                    break;
                }
                count++;
            }
            return count;
        }

        private void MoveBackChar()
        {
            _moveNextStop = true;
        }

        private int ReadChar()
        {
            if (_moveNextStop)
            {
                _moveNextStop = false;
                return _currentChar;
            }
            _lastChar = _currentChar;
            _currentChar = reader.Read();
            if (_currentChar == -1)
            {
                return _currentChar;
            }
            _charIndex++;
            if (_currentChar == '\n' && _lastChar == '\r')
            {
                return ReadChar();
            }
            if (IsNewLine(_currentChar))
            {
                _lineIndex++;
                _columnIndex = 0;
            }
            else
            {
                _columnIndex++;
            }
            return _currentChar;
        }


        private bool IsNewLine(int code)
        {
            return code == '\r' || code == '\n';
        }

        private bool IsOperator(string value)
        {
            return value switch
            {
                "+" or "-" or "*" or "/" or "~" or "^" or "<<" or ">>" or "&"
                or "|" or "%" => true,
                _ => false
            };
        }

        private bool IsDeterminer(string value)
        {
            return value switch
            {
                "==" or "!=" or "&&" or "||" or ">" or "<" or "<=" or ">=" => true,
                _ => false
            };
        }

        private bool IsCompoundOperator(string value)
        {
            return value switch
            {
                "++" or "--" or "+=" or "-=" or "*=" or "/=" or "%=" or "^=" or "~=" => true,
                _ => false
            };
        }

        private bool IsCombinationSymbol(char code)
        {
            return code switch
            {
                '!' or '&' or '=' or '%' or '*' or '+' or '/' or '-' or '.'
                or '<' or '>' or '?' or '^' or '~' => true,
                _ => false
            };
        }

        private bool IsNumeric(int code)
        {
            return code >= '0' && code <= '9';
        }

        private bool IsHexNumeric(int code)
        {
            return IsNumeric(code) || (code >= 'a' && code <= 'f') ||
                    (code >= 'A' && code <= 'F');
        }

        /// <summary>
        /// 是否是大写字母
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool IsUpperAlphabet(int code)
        {
            return code >= 'A' && code <= 'Z';
        }

        /// <summary>
        /// 是否是小写字母
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool IsLowerAlphabet(int code)
        {
            return code >= 'a' && code <= 'z';
        }


        /// <summary>
        /// 是否是字母
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool IsAlphabet(int code)
        {
            return IsUpperAlphabet(code) || IsLowerAlphabet(code);
        }

        /// <summary>
        /// 也包括换行
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool IsWhiteSpace(char code)
        {
            return char.IsWhiteSpace(code);
        }

        private bool IsWhiteSpace(int code)
        {
            return IsWhiteSpace((char)code);
        }

        private bool IsReservedKeywords(string text)
        {
            return text switch
            {
                "!" or "=" or "return" or "if" or "else" or "import" or 
                "struct" or "for" or "break" or "switch" or "fn"
                or "const" or "as" or 
                "namespace"
                or "padding"
                or "include" 
                or "enum"
                or "bitfield"
                or "union"
                or "using"
                or "in" or "out"
                or "le" or "be" => true,
                _ => false
            };
        }


        private bool IsDataType(string text)
        {
            return text switch
            {
                "u8" or "u16" or "u24" or "u32" or "u48" or "u64" or "u96" or "u128"
                or "s8" or "s16" or "s24" or "s32" or "s48" or "s64" or "s96" or "s128"
                or "bool" 
                or "float" or "double" 
                or "char" or "char16"
                or "str" or "auto" => true,
                _ => false
            };
        }

        private Token CreateToken(string value, int beginOffset, int endOffset = 0)
        {
            var type = TokenType.Identifier;
            if (IsReservedKeywords(value))
            {
                type = TokenType.ReservedKeywords;
            }
            else if (IsDataType(value))
            {
                type = TokenType.DataType;
            }
            return CreateToken(type, value, beginOffset, endOffset);
        }

        private Token CreateToken(string value, CursorPosition begin, CursorPosition end)
        {
            var type = TokenType.Identifier;
            if (IsReservedKeywords(value))
            {
                type = TokenType.ReservedKeywords;
            }
            else if (IsDataType(value))
            {
                type = TokenType.DataType;
            }
            return new Token(type, value, begin, end);
        }

        private Token CreateToken(string value, CursorPosition begin)
        {
            return CreateToken(value, begin, CreatePosition());
        }

        private Token CreateToken(TokenType type, string value, CursorPosition begin)
        {
            return new Token(type, value, begin, CreatePosition());
        }


        private Token CreateToken(TokenType type, string value, int beginOffset, int endOffset = 0)
        {
            return new Token(type, value,
                new CursorPosition(_lineIndex, _columnIndex - beginOffset, _charIndex - beginOffset),
                new CursorPosition(_lineIndex, _columnIndex + endOffset, _charIndex + endOffset));
        }

        private Token CreateToken(TokenType type, int beginOffset, int endOffset = 0)
        {
            return CreateToken(type, type.ToString(), beginOffset, endOffset);
        }

        /// <summary>
        /// 往前偏移
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private CursorPosition CreatePosition(int offset)
        {
            return CreatePosition(_lineIndex, _columnIndex -
                offset, _charIndex - offset);
        }

        private CursorPosition CreatePosition(int line, int column, int index)
        {
            return new(line, column, index);
        }

        private CursorPosition CreatePosition()
        {
            return CreatePosition(_moveNextStop ? 1 : 0);
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
