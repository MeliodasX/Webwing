using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myOwnWebServer
{
    class Logger
    {
        private const string logFilePath = @"myOwnWebServer.log";

        public static void DeleteLog()
        {
            File.Delete(logFilePath);
        }
        public static void WriteLog(string logEntry)
        {
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirectoryInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirectoryInfo.Exists) logDirectoryInfo.Create();
            using (FileStream fs = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fs))
                {
                    log.WriteLine(System.DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "\t" + logEntry);
                }
            }
        }
    }
}
