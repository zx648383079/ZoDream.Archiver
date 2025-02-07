using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter
{
    public partial class ObjReader
    {

        public ModelRoot? Read(Stream input)
        {
            var data = ReadObj(input);

            return new ModelRoot();
        }


        
    }
}
