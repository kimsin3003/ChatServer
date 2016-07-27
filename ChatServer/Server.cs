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

        public Server(int port)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            listenSock = null;
            maxClientNum = 10;
            sessionManager = new SessionManager();
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
                ProcessReadableSession();
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
                    Console.WriteLine("Client" + session.id + "(" + session.ip + ")" + " has come");
                }
            }
        }

        private void ProcessReadableSession()
        {
            List<Session> readableSessions;
            readableSessions = sessionManager.GetReadableSessions();

            foreach (Session session in readableSessions)
            {
                ProcessMessage(session);
            }

        }
        
        private void ProcessMessage(Session session)
        {
            Socket socket = session.socket;
            IPAddress ipAddress = session.ip;

            byte[] headerByte;
            byte[] bodyByte;

            if (!ReceiveData(session, out headerByte, Marshal.SizeOf(typeof(Header))))
                return;

            Header header = (Header)Serializer.ByteToStructure(headerByte, typeof(Header));
            short type = (short)header.type;
            int bodyLength = header.length;
            Console.WriteLine("type: " + type + ", length: " + bodyLength);

            if (!ReceiveData(session, out bodyByte, bodyLength))
                return;

            string message = Serializer.BytesToString(bodyByte);
            Console.WriteLine(message);
        }

        private bool ReceiveData(Session session, out byte[] buf, int size)
        {
            buf = new byte[size];
            try
            {
                session.socket.Receive(buf);
            }
            catch (SocketException)
            {
                sessionManager.RemoveSession(session);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sessionManager.RemoveSession(session);
                return false;
            }
            return true;
        }
    }
}
