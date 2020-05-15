using GeraldServiceAutoOperate;
using GeraldServiceAutoOperate.Entity;
using GeraldServiceAutoOperate.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using KillProcessInstaller.Common;

namespace KillProcessInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            MainController operate = new MainController();
            BeforeInstallService(operate);
            InstallService(operate);
        }

        private static void BeforeInstallService(MainController operate)
        {
            Console.WriteLine("正在检测系统是否已安装相关服务...");
            ServiceInfo sc = operate.GetServiceInfo("KillProcess");
            if (sc.isExist)
            {
                if (operate.RunServiceCmd(sc.serviceInstallPath, sc.serviceExeName, CmdFlag.uninstall))
                {
                    Console.WriteLine("原相关服务已成功卸载...");
                    //杀掉服务相关进程，确保卸载成功
                    ProcessInfo p = operate.GetProcessInfo(sc.serviceExeName.Replace(".exe", ""));
                    if (p.isExist)
                    {
                        operate.KillProcessAfterUninstall(p.processName);
                        Console.WriteLine("原相关服务进程已成功结束...");
                    }
                    //卸载后删除服务相关文件夹
                    if (operate.DeleteDir(sc.serviceInstallPath))
                    {
                        Console.WriteLine("原服务程序可执行文件已成功移除...");
                    }
                }
                else
                {
                    Console.WriteLine("原相关服务无法卸载,请联络Gerald.Wang...");
                }
            }
            else
            {
                Console.WriteLine("系统未安装相关服务...");
            }
        }

        private static void InstallService(MainController operate)
        {
            Console.WriteLine("开始下载服务资源文件...");
            if (operate.ExtractResourceFile("KillProcessInstaller.Resource.KillProcessService.exe", @"C:\KillProcessServiceFile\", @"KillProcessService.exe"))
            {
                Console.WriteLine("服务资源下载成功,准备安装...");
                Console.WriteLine("正在安装服务,请稍后...");
                if (operate.RunServiceCmd(@"C:\KillProcessServiceFile\", @"KillProcessService.exe", CmdFlag.install))
                {
                    //Write Log
                    Console.WriteLine("服务安装成功...");
                }
                else
                {
                    //Write Log
                    Console.WriteLine("服务安装失败,请联络Gerald.Wang...");
                }
            }
            else
            {
                Console.WriteLine("服务资源下载失败,请联络Gerald.Wang...");
            }
        }
    }
}
