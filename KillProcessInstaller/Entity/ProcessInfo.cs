using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeraldServiceAutoOperate.Entity
{
    public class ProcessInfo
    {
        public string processName { get; set; }
        public DateTime startTime { get; set; }
        public string VirtualMemorySize { get; set; }
        public string PagedSystemMemorySize { get; set; }
        public string NonpagedSystemMemorySize { get; set; }
        public bool isExist { get; set; } = false;
    }
}
