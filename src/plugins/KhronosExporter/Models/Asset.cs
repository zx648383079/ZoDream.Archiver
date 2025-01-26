using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Asset : ExtraProperties
    {
        public string Copyright {  get; set; }

        public string Generator { get; set; }

        public string Version { get; set; }

        public string MinVersion { get; set; }
    }
}
