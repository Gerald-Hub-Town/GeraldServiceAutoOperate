using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using KillProcessService.Common;
using System.Threading;

namespace KillProcessService
{
    public class TimerMonitor
    {
        Speech speech = new Speech();
        private readonly System.Timers.Timer _timer;

        public TimerMonitor()
        {
            _timer = new System.Timers.Timer(1000 * 50);
            _timer.Elapsed += (sender, EventArgs) =>
            {
                OperateKillProcess("FACTORYworks");
            };
        }

        private string GetCurrHourMinute()
        {
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            string hourText = "", minuteText = "";
            if (hour < 10) 
            {
                hourText = "0" + hour.ToString();
            }
            if (hour > 12)
            {
                hour = hour - 12;
                if (hour < 10)
                {
                    hourText = "0" + hour.ToString();
                }
                else
                {
                    hourText = hour.ToString();
                }
            }
            if (minute < 10)
            {
                minuteText = "0" + minute.ToString();
            }
            else
            {
                minuteText = minute.ToString();
            }
            return hourText + minuteText;
        }

        private string OperateKillProcess(string processName)
        {
            string result = "";
            Process[] processList = Process.GetProcesses();
            try
            {
                foreach (Process processItem in processList)
                {
                    if (processItem.ProcessName.Contains(processName))
                    {
                        string time = GetCurrHourMinute();
                        if (time == "0845" || time == "0145")
                        {
                            speech.SpeechTextRead(0, 60, "请注意,生产管理系统将于15分钟后关闭");
                            Thread.Sleep(2000);
                            speech.SpeechTextRead(0, 60, "请注意,生产管理系统将于15分钟后关闭");
                        }
                        if (time == "0900" || time == "0200")
                        {
                            speech.CountDownRead(0, 60, 10);
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
