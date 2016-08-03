using System;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class ServerMain
    {
        private static bool isclosing = false;
        

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
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

            
        }// end method
        
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    Console.WriteLine("Program being closed!");
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    Console.WriteLine("User is logging off!");
                    break;
            }
            return true;
        }
    }
}
