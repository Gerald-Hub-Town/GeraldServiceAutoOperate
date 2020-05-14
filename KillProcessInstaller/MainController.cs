using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Win32;
using GeraldServiceAutoOperate.Entity;
using GeraldServiceAutoOperate.Common;
using GeraldServiceAutoOperate.Enum;

namespace GeraldServiceAutoOperate
{
    public class MainController
    {
        private bool loadResourceFileFlag = false;
        private bool excuteCmdFlag = false;
        private bool deleteDirFlag = false;
        private string ip;
        private string hostName;
        private Logger _logger;
        private static FileStream fs;

        public MainController()
        {
            this._logger = new Logger();
            this.hostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);//网卡IP地址袭集百合
            this.ip = ipEntry.AddressList[7].ToString();//取一度个IP
        }
        /// <summary>
        /// 获取进程信息
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public ProcessInfo GetProcessInfo(string processName)
        {
            ProcessInfo processInfo = new ProcessInfo();
            Process[] processList = Process.GetProcesses();
            Process process = processList.FirstOrDefault(x => x.ProcessName.Contains(processName));
            if (process != null)
            {
                processInfo.processName = process.ProcessName;
                processInfo.startTime = process.StartTime;
                processInfo.isExist = true;
            }
            return processInfo;
        }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public ServiceInfo GetServiceInfo(string serviceName)
        {
            ServiceInfo serviceInfo = new ServiceInfo();
            ServiceController sc = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == serviceName);
            if (sc != null)
            {
                string key = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
                string path = Registry.LocalMachine.OpenSubKey(key).GetValue("ImagePath").ToString();
                if (!string.IsNullOrEmpty(path))
                {
                    path = path.Replace("\"", string.Empty);
                    FileInfo fi = new FileInfo(path);
                    string exeName = fi.Name.Remove(fi.Name.IndexOf('-'), fi.Name.Length - fi.Name.IndexOf('-')).Trim();
                    serviceInfo.serviceName = sc.ServiceName;
                    serviceInfo.serviceExeName = exeName;
                    serviceInfo.serviceInstallPath = fi.Directory.ToString();
                    serviceInfo.status = sc.Status.ToString();
                    serviceInfo.isExist = true;
                }
            }
            return serviceInfo;
        }

        private static string FormatSize(double size)
        {
            double d = (double)size;
            int i = 0;
            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }
            string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
        }

        /// <summary>
        /// 下载项目嵌入资源文件
        /// </summary>
        /// <param name="resFileName">资源文件名</param>
        /// <param name="outputDir">输出路径</param>
        /// <param name="outputFile">输出文件</param>
        public bool ExtractResourceFile(string resFileName, string outputDir, string outputFile)
        {
            BufferedStream inStream = null;
            FileStream outStream = null;
            if (File.Exists(outputDir + outputFile))
            {
                loadResourceFileFlag = true;
                //若所需下载的资源文件之前已经下载到本机则执行...
            }
            else
            {
                if (Directory.Exists(outputDir) == false)
                {
                    Directory.CreateDirectory(outputDir);
                }
                try
                {
                    Assembly asm = Assembly.GetExecutingAssembly();
                    inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
                    outStream = new FileStream(outputDir + outputFile, FileMode.Create, FileAccess.Write);
                    byte[] buffer = new byte[1024];
                    int length;
                    while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, length);
                    }
                    outStream.Flush();
                    loadResourceFileFlag = true;
                    WriteToLog($"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务资源下载成功", "");
                    //WriteToLog($"\\10.32.36.230\\临时共享盘\\USB\\LuxKillProcess\\KillProcessLog-All-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务资源下载成功", "");
                    //_logger.Default.Process(new Log("Info", $"IP:{ip}---服务资源下载成功", DateTime.Now, "LoadResourceFile"));
                }
                catch (Exception ex)
                {
                    WriteToLog($"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务资源下载失败,详细信息:{ex.ToString()}", "");
                    //WriteToLog($"\\10.32.36.230\\临时共享盘\\USB\\LuxKillProcess\\KillProcessLog-All-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务资源下载失败,详细信息:{ex.ToString()}", "");
                    //_logger.Default.Process(new Log("Info", $"IP:{ip}---服务资源下载失败,详细信息:{ex.ToString()}", DateTime.Now, "LoadResourceFile"));
                }
                finally
                {
                    if (outStream != null)
                    {
                        outStream.Close();
                    }
                    if (inStream != null)
                    {
                        inStream.Close();
                    }
                }
            }
            return loadResourceFileFlag;
        }

        /// <summary>
        /// 对服务可执行程序执行CMD命令
        /// </summary>
        /// <param name="serviceExecutableFilePath">服务可执行文件文件夹</param>
        /// <param name="serviceExeName">服务可执行文件名称</param>
        /// <param name="flag">CMD命令</param>
        public bool RunServiceCmd(string serviceExecutableFilePath, string serviceExeName, CmdFlag flag)
        {
            string excuteCommand = string.Empty;
            switch (flag)
            {
                case CmdFlag.install:
                    excuteCommand = "install";
                    break;
                case CmdFlag.start:
                    excuteCommand = "start";
                    break;
                case CmdFlag.uninstall:
                    excuteCommand = "uninstall";
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }
            return ExcuteCmd(serviceExecutableFilePath, serviceExeName, excuteCommand);
        }

        private bool ExcuteCmd(string serviceExecutableFilePath, string serviceExeName, string cmdFlag)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.Arguments = @"C:\Windows\System32";
            pro.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
            pro.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
            pro.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
            pro.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
            pro.Start();

            try
            {
                string motherDrive = serviceExecutableFilePath.Substring(0, 2);
                string sonDriver = serviceExecutableFilePath.Substring(3, serviceExecutableFilePath.Length - 3);
                string command1 = @"cd\";
                string command2 = motherDrive;
                string command3 = "cd " + sonDriver; // @"cd SourceCode(LUXSHARE)\Nancy\KillProcess\bin\debug"
                string command4 = serviceExeName + " " + cmdFlag;
                string command5 = string.Empty;
                pro.StandardInput.WriteLine(command1);
                pro.StandardInput.WriteLine(command2);
                pro.StandardInput.WriteLine(command3);
                pro.StandardInput.WriteLine(command4);
                if (cmdFlag == "install")
                {
                    command5 = serviceExeName + " " + "start";
                    pro.StandardInput.WriteLine(command5);
                }
                pro.StandardInput.WriteLine("&exit");
                pro.StandardInput.AutoFlush = true;
                pro.StandardInput.WriteLine("exit");
                pro.WaitForExit();
                excuteCmdFlag = true;
                WriteToLog($"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务执行命令成功,执行命令--1.{command1} 2.{command2} 3.{command3} 4.{command4} {command5}", "");
                //WriteToLog($"\\10.32.36.230\\临时共享盘\\USB\\LuxKillProcess\\KillProcessLog-All-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务执行命令成功,执行命令--1.{command1} 2.{command2} 3.{command3} 4.{command4}", "");
                // _logger.Default.Process(new Log("Info", $"IP:{ip}---服务执行命令成功,执行命令--1.{command1} 2.{command2} 3.{command3} 4.{command4}", DateTime.Now, "LoadResourceFile"));
            }
            catch (Exception ex)
            {
                WriteToLog($"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务执行命令失败,详细信息:{ex.ToString()}", "");
                //WriteToLog($"\\10.32.36.230\\临时共享盘\\USB\\LuxKillProcess\\KillProcessLog-All-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务执行命令失败,详细信息:{ex.ToString()}", "");
                //_logger.Default.Process(new Log("Info", $"IP:{ip}---服务执行命令失败,详细信息:{ex.ToString()}", DateTime.Now, "LoadResourceFile"));
            }
            finally
            {
                pro.Close();
            }
            return excuteCmdFlag;
        }

        /// <summary>
        /// KILL系统进程
        /// </summary>
        /// <param name="processName"></param>
        public void KillProcessAfterUninstall(string processName)
        {
            Process[] processList = Process.GetProcesses();
            foreach (Process p in processList)
            {
                if (p.ProcessName == processName)
                {
                    p.Kill();
                }
            }
        }

        /// <summary>
        /// 删除文件夹及文件夹内所有文件
        /// </summary>
        /// <param name="srcPath"></param>
        /// <returns></returns>
        public bool DeleteDir(string srcPath)
        {
            string flag = string.Empty;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);
                    }
                    else
                    {
                        File.Delete(i.FullName);
                    }
                }
                Directory.Delete(srcPath);
                deleteDirFlag = true;
            }
            catch (Exception ex)
            {
                //Write Log
            }
            return deleteDirFlag;
        }

        private void VirtualSleep()
        {
            Thread.Sleep(500);
        }

        private bool WriteToLog(string outLogPath, string logLevel, string message, string errorMessage)
        {
            bool result = true;
            string localLogPath = $"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt";
            if (!File.Exists(outLogPath))
            {
                fs = new FileStream(outLogPath, FileMode.Create, FileAccess.ReadWrite);
                File.SetAttributes(outLogPath, FileAttributes.Normal);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                try
                {
                    sw.WriteLine("日志记录时间：" + DateTime.Now.ToString());
                    sw.WriteLine("日志级别：" + logLevel);
                    sw.WriteLine("日志信息：" + message);
                    sw.WriteLine("错误信息：" + errorMessage);
                    sw.WriteLine("======================================================================================================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("文件处理失败{0}", ex.ToString());
                    result = false;
                }
                finally
                {
                    sw.Close();
                    fs.Close();
                }
            }
            else
            {
                FileStream fs = null;
                string filePath = outLogPath;
                //将待写的入数据从字符串转换为字节数组  
                Encoding encoder = Encoding.UTF8;
                byte[] dateByte = encoder.GetBytes("\n\r" + "日志记录时间：" + DateTime.Now.ToString());
                byte[] levelByte = encoder.GetBytes("\r" + "日志级别：" + logLevel);
                byte[] messageByte = encoder.GetBytes("\r" + "日志信息：" + message);
                byte[] errorMessageByte = encoder.GetBytes("\r" + "错误信息：" + errorMessage);
                byte[] line = encoder.GetBytes("\r" + "======================================================================================================");
                //byte[] bytes = encoder.GetBytes("\r" + "当前时间: " + DateTime.Now.ToString());//"\n\r"隔行显示
                try
                {
                    fs = File.OpenWrite(filePath);//等价于fs = File.Open(filePath, FileMode.Append, FileAccess.ReadWrite); 
                    //设定书写的開始位置为文件的末尾  
                    fs.Position = fs.Length;
                    //将待写入内容追加到文件末尾  
                    fs.Write(dateByte, 0, dateByte.Length);
                    fs.Write(levelByte, 0, levelByte.Length);
                    fs.Write(messageByte, 0, messageByte.Length);
                    fs.Write(errorMessageByte, 0, errorMessageByte.Length);
                    fs.Write(line, 0, line.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("文件打开失败{0}", ex.ToString());
                    result = false;
                }
                finally
                {
                    fs.Close();
                }
            }
            return result;
        }
    }
}
