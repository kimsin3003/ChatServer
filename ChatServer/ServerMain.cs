using System;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class ServerMain
    {
        
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
        
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);
        
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        static void Main(string[] args)
        {
            Console.Title = "Chat Server " + Int32.Parse(args[0]);
            Console.ForegroundColor = ConsoleColor.Green;
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


            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
            Server server = new Server(Int32.Parse(args[0]), "10.100.58.3", 25389, 100);
            server.Start();
            server.ShutDown();

            
        }
        
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            Console.WriteLine("Shutting Down Server");
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    try
                    {
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    Console.WriteLine("Program being closed!");
                    try
                    {
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Console.WriteLine("User is logging off!");
                    break;
            }
            return true;
        }
    }
}
