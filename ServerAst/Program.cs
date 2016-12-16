using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace ServerAst
{
    class MainClass
    {
        public static string SendResponse(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
        }
        public static void Main(string[] args)
        {
            string basePath = "";

            // For testing purposes...
            if (args.Length == 0)
            {
                Console.WriteLine("No commandline arguments, harcoded debug mode...");
                Console.WriteLine("arg example : 'c:\\\\data\\\\assetbundles\\'\n");
            }
            else
            {
                basePath = args[0];
            }

            bool detailedLogging = false;
            int port = 7888;

            Console.WriteLine("Starting up asset bundle server. Upgraded to working multi request.", port);
            Console.WriteLine("Port: {0}", port);
            Console.WriteLine("Directory: {0}", basePath);

            WebServer ws = new WebServer(SendResponse, string.Format("http://*:{0}/", port));

            ws.Run(basePath, detailedLogging);

            Console.WriteLine("input 'qqq' to exit program.");
            while (true)
            {
                string str = Console.ReadLine();

                if (str == "qqq")
                    break;
            }
            ws.Stop();
            Console.WriteLine("server stop complete. Press any key to close window.");
            Console.ReadKey();

            //HttpListener listener = new HttpListener();
            //listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            //listener.Start();
            //while (true)
            //{
            //    Console.WriteLine("Waiting for request...");
            //    HttpListenerContext context = listener.GetContext();
            //    WriteFile(context, basePath, detailedLogging);
            //}
        }
    }
}
