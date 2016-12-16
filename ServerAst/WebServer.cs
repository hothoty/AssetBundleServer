using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServerAst
{
    class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example
            // "http://localhost:8080/index/"
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method) { }

        public void Run(string basePath, bool detailedLogging)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            WriteFile(ctx, basePath, detailedLogging);
                            
                        }, _listener.GetContext());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        void WriteFile(HttpListenerContext ctx, string basePath, bool detailedLogging)
        {
            HttpListenerRequest request = ctx.Request;
            string rawUrl = request.RawUrl;
            string path = basePath + rawUrl;

            if (detailedLogging)
                Console.WriteLine("Requesting file: '{0}'. Relative url: {1} Full url: '{2} AssetBundleDirectory: '{3}''", path, request.RawUrl, request.Url, basePath);
            else
                Console.Write("Requesting file: '{0}' ... ", request.RawUrl);

            var response = ctx.Response;
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    string filename = Path.GetFileName(path);
                    //response is HttpListenerContext.Response...
                    response.ContentLength64 = fs.Length;
                    response.SendChunked = false;
                    response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                    response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                    byte[] buffer = new byte[64 * 1024];
                    int read;
                    using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                    {
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, read);
                            bw.Flush(); //seems to have no effect
                        }

                        bw.Close();
                    }

                    Console.WriteLine("completed.");
                    //				response.StatusCode = (int)HttpStatusCode.OK;
                    //				response.StatusDescription = "OK";
                    response.OutputStream.Close();
                    response.Close();
                }
            }
            catch (System.Exception exc)
            {
                Console.WriteLine(" failed.");
                Console.WriteLine("Requested file failed: '{0}'. Relative url: {1} Full url: '{2} AssetBundleDirectory: '{3}''", path, request.RawUrl, request.Url, basePath);
                Console.WriteLine("Exception {0}: {1}'", exc.GetType(), exc.Message);
                response.Abort();
            }
        }
    }
}
