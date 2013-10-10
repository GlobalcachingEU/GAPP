using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GAPPChatService
{
    class Program
    {
        static ManualResetEvent _stop = null;

        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing Service...");

            // The service configuration is loaded from app.config
            using (ChatService host = new ChatService())
            {
                host.Start();

                Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
                _stop = new ManualResetEvent(false);

                //Console.WriteLine("[CTRL+C] om te stoppen");
                //_stop.WaitOne();
                Console.WriteLine("Druk op een toets om te stoppen");
                Console.ReadKey();

                host.Dispose();
                Console.WriteLine("Closing service...");
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            //bye bye
            if (_stop != null)
            {
                _stop.Set();
            }
        }
    }
}
