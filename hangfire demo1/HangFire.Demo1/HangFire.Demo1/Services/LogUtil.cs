using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangFire.Demo1.Services
{
    public class LogUtil
    {
        private static string DirPath = string.Empty;
        private const string Info_Log = "trace-info.log";
        private const string Error_Log = "trace-error.log";
        private const long FileSize = 2097152;

        static LogUtil()
        {
            DirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"logs");
        }

        public static void WriteInfo(IBillType billType, string request, string response)
        {
            try
            {
                LogUtil logUtil = new LogUtil();
                logUtil.Write(billType, request, response, Info_Log);
            }
            catch
            {
                ;
            }
        }

        public static void WriteError(IBillType billType, string error)
        {
            LogUtil logUtil = new LogUtil();
            logUtil.Write(billType, string.Empty, error, Error_Log);
        }

        public static void WriteError(IBillType billType, string request, string response)
        {
            LogUtil logUtil = new LogUtil();
            logUtil.Write(billType, request, response, Error_Log);
        }

        public static void WriteError(IBillType billType, string request, Exception ex, string error = "")
        {
            LogUtil logUtil = new LogUtil();
            logUtil.Write(billType, request, string.Format("StackTrace:{0}Exception:{1}{2}", ex.StackTrace, ex.ToString(), error), Error_Log);
        }
        private void Write(IBillType billType, string request, string response, string fileName)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(DirPath, billType.BillType)))
                {
                    Directory.CreateDirectory(Path.Combine(DirPath, billType.BillType));
                }
                string filePath = Path.Combine(DirPath, billType.BillType + @"\" + fileName);
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length >= FileSize)
                    {
                        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            stream.Flush();

                            //备份数据
                            using (FileStream newStream = new FileStream(Path.Combine(fileInfo.DirectoryName, DateTime.Now.ToString("yyyyMMdd") + fileName),
                                FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                newStream.Write(buffer, 0, buffer.Length);
                                newStream.Flush();
                            }
                        }
                        //清空数据
                        File.WriteAllText(filePath, string.Empty, UTF8Encoding.UTF8);
                    }
                }
                var sj = string.Format("DateTime:{1}{0}Request:{2}{0}Response:{3}{0}",
                    Environment.NewLine, DateTime.Now.ToString(), request, response);
                using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] buffer = UTF8Encoding.UTF8.GetBytes(sj);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }
            catch
            {
                ;
            }
        }
        public static void Write(string error)
        {
            LogUtil logUtil = new LogUtil();
            logUtil.WriteSysError(error);
        }
        private void WriteSysError(string error)
        {
            if (!Directory.Exists(Path.Combine(DirPath, "System")))
            {
                Directory.CreateDirectory(Path.Combine(DirPath, "System"));
            }
            if (!Directory.Exists(Path.Combine(DirPath, @"System\" + DateTime.Now.ToString("yyyyMMdd"))))
            {
                Directory.CreateDirectory(Path.Combine(DirPath, @"System\" + DateTime.Now.ToString("yyyyMMdd")));
            }
            string filePath = Path.Combine(DirPath, @"System\" + DateTime.Now.ToString("yyyyMMdd") + @"\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".log");
            using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = UTF8Encoding.UTF8.GetBytes(error);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }
    }
}
