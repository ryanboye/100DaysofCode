using System;
using System.Net;
using System.Text;
using System.Threading;

namespace Web_Crawler
{
    public class DummyServer
    {
        private readonly int _port;
        private readonly Thread _serverThread;
        private long _counter;
        private HttpListener _listener;

        public DummyServer(int port)
        {
            _counter = 0;
            _port = port;
            _serverThread = new Thread(Listen);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }
        
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Process(HttpListenerContext context)
        {
            var counter = Interlocked.Increment(ref _counter);
            var response = Encoding.ASCII.GetBytes("<html><a href=\"/" + counter + "\">next</a></html>");
            context.Response.StatusCode = 200;
            context.Response.OutputStream.WriteAsync(response, 0, response.Length).ContinueWith((t) =>
            {
                context.Response.OutputStream.FlushAsync().ContinueWith((g) =>
                {
                    context.Response.OutputStream.Close();
                });
            });
 
            
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    var context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}