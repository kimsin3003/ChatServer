using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace ChatServer
{
    class Server
    {
        private IPEndPoint ipEndPoint;
        private Socket listenSock;
        private int maxClientNum;
        private FBSessionProcessor fbSessionProcessor;
        private CFSessionProcessor cfSessionProcessor;
        private Thread acceptingThread;
        
        private string backEndIp;
        private int backEndPort;
        private Session backEndSession;

        public Server(int port, string backEndIp, int backEndPort)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            listenSock = null;
            backEndSession = null;
            maxClientNum = 100;
            SessionManager.GetInstance().Init(11000);
            fbSessionProcessor = new FBSessionProcessor();
            cfSessionProcessor = new CFSessionProcessor();

            this.backEndIp = backEndIp;
            this.backEndPort = backEndPort;
        }

        public void ShutDown()
        {
            cfSessionProcessor = null;
            fbSessionProcessor = null;

            listenSock.Shutdown(SocketShutdown.Both);
            listenSock.Close();
            acceptingThread.Join();

            SessionManager.ShutDown();
            RoomManager.ShutDown();


            Console.WriteLine("Server has closed safely.");
        }

        public int MaxClientNum
        {
            get { return maxClientNum; }
        }

        public void ConnectToBackEnd()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(backEndIp);
            IPAddress ipAddr = ipHost.AddressList[1];     //In index 0, there's ipv6 address.
            
            Socket backEndSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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

                backEndSession = SessionManager.GetInstance().MakeNewSession(backEndSock);
                Console.WriteLine("Connected to BackEnd server");
                return;
            }
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
            acceptingThread = new Thread(new ThreadStart(AcceptingProcess));
            acceptingThread.Start();

            while (true)
            {
                MainProcess();
            }
        }

        private void AcceptingProcess()
        {
            while (true)
            {
                if (listenSock == null)
                    return;
                if (listenSock.Poll(10, SelectMode.SelectRead))
                {
                    Socket newClient = listenSock.Accept();
                    Session session = SessionManager.GetInstance().MakeNewSession(newClient);
                    Console.WriteLine("Client(" + session.sessionId + ", " + session.Ip + ")" + " is Connected");
                }
            }
        }

        private void MainProcess()
        {
            List<Session> readableSessions = SessionManager.GetInstance().GetReadableSessions();

            foreach (Session session in readableSessions)
            {
                
                if(session == backEndSession)
                {
                    if (fbSessionProcessor.ProcessReadableSession(session))
                    {
                        if (!session.isConnected)
                        {
                            ConnectToBackEnd();
                        }
                    }
                }
                else
                {
                    cfSessionProcessor.ProcessReadableSession(session, backEndSession);
                }
            }

            SessionManager.GetInstance().RemoveClosedSessions();
        }
    }
}
