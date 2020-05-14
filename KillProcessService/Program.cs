using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace KillProcessService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<TimerMonitor>(s =>
                {
                    s.ConstructUsing(name => new TimerMonitor());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetServiceName("KillProcess");
                x.SetDisplayName("KillProcess");
                x.SetDescription("定时结束系统进程");
            });
        }
    }
}
