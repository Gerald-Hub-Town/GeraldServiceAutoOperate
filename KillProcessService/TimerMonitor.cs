using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KillProcessService
{
    public class TimerMonitor
    {
        private readonly Timer _timer;
        private string time1 = "0900";
        private string time2 = "1700";
        private string time3 = "2100";
        public TimerMonitor()
        {
            _timer = new Timer(1000 * 50);
            _timer.Elapsed += (sender, EventArgs) =>
            {
                OperateKillProcess();
            };
        }

        private string OperateKillProcess()
        {
            string result = "";
            Process[] processList = Process.GetProcesses();
            try
            {
                foreach (Process processItem in processList)
                {
                    if (processItem.ProcessName.Contains("FACTORYworks"))
                    {
                        string dspNow = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
                        if (dspNow == time1 || dspNow == time2 || dspNow == time3) 
                        {
                            processItem.Kill();
                            processItem.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ("Fail:" + ex.ToString());
            }
            return result;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
