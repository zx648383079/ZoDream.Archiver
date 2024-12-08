﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ZoDream.LuaDecompiler.Models
{
    public partial class JitOperandExtractor(LuaVersion version) : IOperandExtractor
    {
        public IOperandCode Extract([Length(4,4)] byte[] buffer)
        {
            var op = ParseOperand(buffer[0], version);
            var a = buffer[1];
            var res = new JitOperandCode(op, a);
            if (IsABCFormat(op))
            {
                res.B = buffer[2];
                res.C = buffer[3];
            } else
            {
                res.D = buffer[2] | (uint)(buffer[3] << 8);
            }
            return res;
        }

        public IOperandCode Extract(uint code)
        {
            return Extract(BitConverter.GetBytes(code));
        }
    }
}