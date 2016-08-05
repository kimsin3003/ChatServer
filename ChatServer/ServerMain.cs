﻿using System;
using System.Threading;

namespace ChatServer
{
    class ServerMain
    {
        
        
        static void Main(string[] args)
        {

            try
            {
                Console.Title = "Chat Server " + Int32.Parse(args[0]);
                Console.ForegroundColor = ConsoleColor.Green;
                if (args.Length == 1 && args[0] == "-help")
                {
                    Console.WriteLine("listeningPort backEndIp backEndPort maxClientNum");
                    return;
                }

                if (args.Length == 0)
                {
                    Console.WriteLine("listeningPort backEndIp backEndPort maxClientNum");
                    return;
                }

                Thread exitInputThread = new Thread(new ThreadStart(ExitInput));
                exitInputThread.Start();
                Console.WriteLine("To Exit, Press ESCAPE.");
                Console.WriteLine("In other way, there's no prove for complete exit and sending FIN");
                Server server = new Server(Int32.Parse(args[0]), args[1], Int32.Parse(args[2]), Int32.Parse(args[3]));
                server.Start();
                server.ShutDown();
            }
            catch(Exception e)
            {
                Environment.Exit(0);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void ExitInput()
        {
            ConsoleKeyInfo input = Console.ReadKey(false);

            if ((input.Key & ConsoleKey.Escape) != 0)
                Environment.Exit(0);
        }
        
    }
}
