using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeraldServiceAutoOperate.Entity
{
    public class ServiceInfo
    {
        public string serviceName { get; set; }
        public string serviceExeName { get; set; }
        public DateTime startTime { get; set; }
        public string serviceInstallPath { get; set; }
        public string status { get; set; }
        public bool isExist { get; set; } = false;
}
}
