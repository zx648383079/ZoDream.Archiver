﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Language
{
    public interface ICodeWriter : IDisposable
    {
        /// <summary>
        /// 当前的行缩进
        /// </summary>
        public int Indent { get; }

        public ICodeWriter Write(string? text);
        /// <summary>
        /// 调用 string.Format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ICodeWriter WriteFormat([StringSyntax("CompositeFormat")] string format, params object?[] args);
        /// <summary>
        /// 写入以引号包围的字符串
        /// </summary>
        /// <param name="text">需要手动进行内部引号的转义</param>
        /// <param name="outDoubleQuote">是否是双引号，否则单引号</param>
        /// <returns></returns>
        public ICodeWriter Write(string text, bool outDoubleQuote);
        public ICodeWriter Write(byte val);
        public ICodeWriter Write(bool val);
        public ICodeWriter Write(int val);
        public ICodeWriter Write(short val);
        public ICodeWriter Write(char val);
        public ICodeWriter Write(uint val);
        public ICodeWriter Write(float val);
        public ICodeWriter Write(long val);
        public ICodeWriter Write(double val);
        public ICodeWriter Write(object? val);
        /// <summary>
        /// 写入字符串，请确认编码是 UTF8
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ICodeWriter Write(byte[] buffer, int offset, int count);
        /// <summary>
        /// 写入字符串，请确认编码是 UTF8
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public ICodeWriter Write(byte[] buffer);
        public ICodeWriter Write(char val, int repeatCount);
        /// <summary>
        /// 写入内容再写入换行符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ICodeWriter WriteLine(string text);
        /// <summary>
        /// 写换行符
        /// </summary>
        /// <returns></returns>
        public ICodeWriter WriteLine();
        /// <summary>
        /// 写换行符，同时是否加入缩进
        /// </summary>
        /// <param name="autoIndent">是否加入缩进</param>
        /// <returns></returns>
        public ICodeWriter WriteLine(bool autoIndent);
        /// <summary>
        /// 写入当前行缩进， 不改变当前行缩进
        /// </summary>
        public ICodeWriter WriteIndent();
        /// <summary>
        /// 增加一个行缩进, 并写入
        /// </summary>
        /// <returns></returns>
        public ICodeWriter WriteIncIndent();
        /// <summary>
        /// 把行缩进修改为并写人
        /// </summary>
        /// <param name="indent"></param>
        public ICodeWriter WriteIndent(int indent);
        /// <summary>
        /// 减少1个单位行缩进
        /// </summary>
        public ICodeWriter WriteOutdent();

        public void Flush();
        
    }
}
