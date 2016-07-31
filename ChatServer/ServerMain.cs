using System;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class ServerMain
    {
        static void Main(string[] args)
        {
            Server server = new Server(11000, "10.100.58.7", 11000);
            SessionManager.GetInstance().Init(11000);
            server.Start();
            server.ShutDown();
        }
    }
}
