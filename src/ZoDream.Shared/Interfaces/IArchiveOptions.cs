using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveOptions
    {
        public bool LeaveStreamOpen { get; }
        public bool LookForHeader { get; }
        public string? Password { get;}
        public string? Dictionary { get; }
    }
}
