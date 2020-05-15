using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillProcessInstaller.Common
{
    public class OperateSharedFolder
    {
        private FileStream fs;
        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="path">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        public bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "net use " + path;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (string.IsNullOrEmpty(errormsg))
                {
                    Flag = true;
                }
                else
                {
                    throw new Exception(errormsg);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }

        /// <summary>
        /// 从远程服务器下载文件到本地
        /// </summary>
        /// <param name="src">下载到本地后的文件路径，不包含文件的扩展名</param>
        /// <param name="localFileName">下载到本地后文件扩展名</param>
        /// <param name="dst">远程服务器路径（共享文件夹路径）</param>
        /// <param name="remoteFileName">远程服务器（共享文件夹）中的文件扩展名</param>
        /// <returns></returns>
        public string TransportRemoteToLocal(string src, string localFileName, string dst, string remoteFileName)  //src：下载到本地后的文件路径     dst：远程服务器路径    fileName:远程服务器dst路径下的文件名
        {
            //测试用例 : TransportRemoteToLocal(@"C:\LuxKillProcess\", "GeraldTest.exe", @"\\10.32.36.230\临时共享盘\USB\LuxProcess\", "LuxKillProcess.exe")
            string result = string.Empty;
            string remoteFileFullName = dst + remoteFileName;
            string localFileFullName = src + localFileName;
            if (File.Exists(remoteFileFullName))
            {
                //string path = @"C:\LuxKillProcess\";
                if (!Directory.Exists(src))
                {
                    Directory.CreateDirectory(src);
                }
                try
                {
                    if (!File.Exists(localFileFullName))
                    {
                        FileStream inFileStream = new FileStream(remoteFileFullName, FileMode.Open);
                        FileStream outFileStream = new FileStream(localFileFullName, FileMode.OpenOrCreate);
                        byte[] buf = new byte[inFileStream.Length];
                        int byteCount;
                        while ((byteCount = inFileStream.Read(buf, 0, buf.Length)) > 0)
                        {
                            outFileStream.Write(buf, 0, byteCount);
                        }
                        inFileStream.Flush();
                        inFileStream.Close();
                        outFileStream.Flush();
                        outFileStream.Close();
                    }
                    else
                        result = $"本地已存在该文件 {localFileFullName}";
                }
                catch (Exception ex)
                {
                    result = $"异常 {ex.ToString()}";
                }
            }
            else
                result = $"共享文件夹不存在文件 {remoteFileFullName}";
            return result;
        }

        /// <summary>
        /// 将本地文件上传到远程服务器共享目录
        /// </summary>
        /// <param name="src">本地文件的绝对路径，包含扩展名</param>
        /// <param name="dst">远程服务器共享文件路径，不包含文件扩展名</param>
        /// <param name="fileName">上传到远程服务器后的文件扩展名</param>
        public string TransportLocalToRemote(string src, string dst, string fileName)    //src
        {
            string result = string.Empty;
            if (File.Exists(src))
            {
                FileStream inFileStream = new FileStream(src, FileMode.Open);

                //判断上传到的远程服务器路径文件夹是否存在
                if (!Directory.Exists(dst))
                {
                    Directory.CreateDirectory(dst);
                }
                try
                {
                    dst = dst + fileName;//上传到远程服务器后的文件完整扩展名
                    FileStream outFileStream = new FileStream(dst, FileMode.OpenOrCreate);
                    byte[] buf = new byte[inFileStream.Length];
                    int byteCount;
                    while ((byteCount = inFileStream.Read(buf, 0, buf.Length)) > 0)
                    {
                        outFileStream.Write(buf, 0, byteCount);
                    }
                    inFileStream.Flush();
                    inFileStream.Close();
                    outFileStream.Flush();
                    outFileStream.Close();
                }
                catch (Exception ex)
                {
                    result = $"异常 {ex.ToString()}";
                }
            }
            else
                result = $"不存在本地文件 {src}";
            return result;
        }

        /// <summary>
        /// 记录系统操作Log信息
        /// </summary>
        /// <param name="outLogPath">日志输出全路径，含文件扩展名称</param>
        /// <param name="logLevel">日志级别</param>
        /// <param name="message">日志信息</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns></returns>
        public bool WriteToLog(string outLogPath, string logLevel, string message, string errorMessage)
        {
            //写入服务器共享文件夹测试用例 : WriteToLog(@"\\10.32.36.230\临时共享盘\USB\LuxProcess\" + $"KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", "测试远程服务器Log写入", "");
            //写入本地测试用例 : WriteToLog($"C:\\KillProcessLog-Own-{DateTime.Now.ToString("yyyyMMdd")}.txt", "Info", $"IP:{ip}---服务资源下载成功", "");
            bool result = true;
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
