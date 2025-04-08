using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace ZoDream.Shared.Language
{
    public class CodeWriter(TextWriter writer) : ICodeWriter
    {
        private const char IndentChar = ' ';
        public CodeWriter(Stream output)
            : this(new StreamWriter(output, Encoding.UTF8))
        {
            
        }

        public CodeWriter(StringBuilder builder)
            : this(new StringWriter(builder))
        {
        }

        public CodeWriter()
            : this(new StringWriter())
        {
        }

        public int Indent { get; set; }

        public void Dispose()
        {
            writer.Dispose();
        }

        public ICodeWriter Write(string? text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                writer.Write(text);
            }
            return this;
        }

        public ICodeWriter WriteFormat([StringSyntax("CompositeFormat")] string format, params object?[] args)
        {
            writer.Write(string.Format(format, args));
            return this;
        }

        public ICodeWriter Write(string text, bool outDoubleQuote)
        {
            var code = outDoubleQuote ? '\'' : '"';
            return Write(code).Write(text).Write(code);
        }

        public ICodeWriter Write(byte val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(bool val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(int val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(short val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(char val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(uint val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(float val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(long val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(double val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter Write(char val, int repeatCount)
        {
            return Write(new string(val, repeatCount));
        }

        public ICodeWriter Write(byte[] buffer, int offset, int count)
        {
            writer.Write(writer.Encoding.GetString(buffer, offset, count));
            return this;
        }

        public ICodeWriter Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        public ICodeWriter WriteIndent()
        {
            if (Indent > 0)
            {
                Write(IndentChar, Indent * (IndentChar == ' ' ? 4 : 1));
            }
            return this;
        }

        public ICodeWriter WriteIndent(int indent)
        {
            Indent = Math.Max(indent, 0);
            return WriteIndent();
        }

        public ICodeWriter WriteLine(string text)
        {
            writer.WriteLine(text);
            return this;
        }

        public ICodeWriter WriteLine()
        {
            writer.WriteLine();
            return this;
        }

        public ICodeWriter WriteOutdent()
        {
            return WriteIndent(Indent - 1);
        }

        public ICodeWriter Write(object? val)
        {
            writer.Write(val);
            return this;
        }

        public ICodeWriter WriteIncIndent()
        {
            Indent++;
            return WriteIndent();
        }

        public void Flush()
        {
            writer.Flush();
        }

        public override string? ToString()
        {
            return writer.ToString();
        }
    }
}
