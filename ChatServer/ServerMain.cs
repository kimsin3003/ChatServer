using System;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class ServerMain
    {
        static void Main(string[] args)
        {

            if (args.Length == 1 && args[0] == "-help")
            {
                Console.WriteLine("port ");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("not enough argumensts");
                return;
            }


            Console.WriteLine("Start Server");
            Server server = new Server(Int32.Parse(args[0]), "10.100.58.3", 15389, 100);
            server.Start();
            server.ShutDown();
        }
    }
}
