using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class Server
    {
        private IPEndPoint ipEndPoint;
        private Socket listenSock;
        private int maxClientNum;
        private SessionManager sessionManager;
        private Thread acceptingThread;
        
        private string backEndIp;
        private int backEndPort;
        private Socket backEndSock;

        public Server(int port, string backEndIp, int backEndPort)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            listenSock = null;
            backEndSock = null;
            maxClientNum = 10;
            sessionManager = new SessionManager();

            this.backEndIp = backEndIp;
            this.backEndPort = backEndPort;
        }

        public void ConnectToBackEnd()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(backEndIp);
            IPAddress ipAddr = ipHost.AddressList[1];     //In index 0, there's ipv6 address.
            
            backEndSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while(true)
            {
                try
                {
                    backEndSock.Connect(new IPEndPoint(ipAddr, backEndPort));
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("IP or port number is null");
                    return;
                }
                catch (SocketException)
                {
                    Console.WriteLine("BackEnd Server is Off");
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                sessionManager.AddSession(backEndSock);
                Console.WriteLine("Connected to BackEnd server");
                return;
            }
        }

        public void ShutDown()
        {
            listenSock.Shutdown(SocketShutdown.Both);
            listenSock.Close();
        }

        public void StartListen()
        {
            listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSock.Bind(ipEndPoint);

                try
                {
                    listenSock.Listen(maxClientNum);
                    Console.WriteLine("Start Listening");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        public void Start()
        {

            StartListen();
            acceptingThread = new Thread(new ThreadStart(Listen));
            acceptingThread.Start();

            while (true)
            {
                MainProcess();
            }
        }

        private void Listen()
        {
            while (true)
            {
                if (listenSock == null)
                    return;
                if (listenSock.Poll(10, SelectMode.SelectRead))
                {
                    Socket newClient = listenSock.Accept();
                    Session session = sessionManager.AddSession(newClient);
                    Console.WriteLine(session.id + "(" + session.sessionId + ", " + session.ip + ")" + " has come");
                }
            }
        }

        private void MainProcess()
        {
            List<Session> readableSessions;
            readableSessions = sessionManager.GetReadableSessions();

            foreach (Session session in readableSessions)
            {
                
                if(session.socket == backEndSock)
                {
                    SessionProcessor sp = new FBSessionProcessor(sessionManager);
                    if (sp.ProcessReadableSession(session))
                    {
                        if (session == null)
                        {
                            ConnectToBackEnd();
                        }
                    }
                }
                else
                {
                    SessionProcessor sp = new CFSessionProcessor(sessionManager);
                    sp.ProcessReadableSession(session);
                }
            }

        }
    }
}
