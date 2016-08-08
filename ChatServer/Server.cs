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
        private int listeningPort;

        public Server(int listeningPort, string backEndIp, int backEndPort, int maxClientNum)
        {
            this.listeningPort = listeningPort;
            ipEndPoint = new IPEndPoint(IPAddress.Any, listeningPort);
            listenSock = null;
            backEndSession = null;
            this.maxClientNum = maxClientNum;
            SessionManager.GetInstance().Init(maxClientNum, listeningPort);
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

        public void ConnectToBackEnd()
        {
            Console.WriteLine("Connecting To BackEnd Server...");
            
            Socket backEndSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (true)
            {
                try
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(backEndIp);
                    IPAddress ipAddr = ipHost.AddressList[0];
                    backEndSock.Connect(new IPEndPoint(ipAddr, backEndPort));
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("IP or port number is null");
                    return;
                }
                catch (SocketException)
                {
                    Console.WriteLine("Where is he??");
                    Thread.Sleep(1000);
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                Console.WriteLine("Making BackEndSession");

                if ((backEndSession = SessionManager.GetInstance().MakeNewSession(backEndSock)) == null)
                {
                    SessionManager.GetInstance().Reset();
                    RoomManager.GetInstance().Reset();
                    backEndSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    continue;
                }
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
            Console.WriteLine("Start Server. port: " + listeningPort);
            ConnectToBackEnd();
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
                if (listenSock.Poll(1000, SelectMode.SelectRead))
                {
                    Socket newClient = listenSock.Accept();
                    Session session = SessionManager.GetInstance().MakeNewSession(newClient);
                    if (session == null)
                    {
                        continue;
                    }
                    Console.WriteLine("Client(" + session.sessionId + ", " + session.ip + ")" + " is Connected");
                }
            }
        }

        private void MainProcess()
        {
            HealthCheckProcess();
            ReadProcess();
        }

        private void HealthCheckProcess()
        {
            List<Session> timedoutSessions = SessionManager.GetInstance().GetTimedoutSessions();
            foreach (Session session in timedoutSessions)
            {
                if (session.socket == backEndSession?.socket)
                {
                    fbSessionProcessor.ProcessTimeoutSession(session);

                    if (!session.isConnected)
                    {
                        Console.WriteLine("Backend Server is down");
                        SessionManager.GetInstance().Reset();
                        RoomManager.GetInstance().Reset();
                        ConnectToBackEnd();
                    }
                }
                else
                {
                    cfSessionProcessor.ProcessTimeoutSession(session, backEndSession);
                }
            }
            SessionManager.GetInstance().RemoveClosedSessions();
        }

        private void ReadProcess()
        {
            List<Session> readableSessions = SessionManager.GetInstance().GetReadableSessions();
            foreach (Session session in readableSessions)
            {
                if (session.socket == backEndSession.socket)
                {
                    fbSessionProcessor.ProcessReadableSession(session);

                    if (!session.isConnected)
                    {
                        Console.WriteLine("Backend Server is down");
                        SessionManager.GetInstance().Reset();
                        RoomManager.GetInstance().Reset();
                        ConnectToBackEnd();
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
