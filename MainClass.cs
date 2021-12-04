using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myOwnWebServer
{
    class MainClass
    {
        static void Main(string[] args)
        {
            String addr = "";
            int port = 0;
            String directory = "";

            Logger.DeleteLog();
            if (args.Length <= 2)
            {
                Logger.WriteLog("[Exception] No arguments provided");
            }
            else {
                try
                {
                    directory = args[0].Split('=')[1];
                    addr = args[1].Split('=')[1];
                    port = int.Parse(args[2].Split('=')[1]);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("[Exception] " + e);
                    return;
                }
                WebServer server = new WebServer(addr, port, directory);
            }
        }
    }
}
