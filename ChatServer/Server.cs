using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace EchoServer
{
    class Server
    {
        private IPEndPoint m_ipEndPoint;
        private Socket m_listenSock;
        private int m_maxClientNum;
        private SessionManager m_sessionManager;
        Thread m_acceptingThread;

        public Server(int port)
        {
            m_ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            m_listenSock = null;
            m_maxClientNum = 10;
            m_sessionManager = new SessionManager();
        }

        public void ShutDown()
        {
            m_listenSock.Shutdown(SocketShutdown.Both);
            m_listenSock.Close();
        }

        public void StartListen()
        {
            m_listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_listenSock.Bind(m_ipEndPoint);

                try
                {
                    m_listenSock.Listen(m_maxClientNum);
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
            m_acceptingThread = new Thread(new ThreadStart(Listen));
            m_acceptingThread.Start();

            while (true)
            {
                ProcessReadableSession();
            }
        }

        private void Listen()
        {
            while (true)
            {
                if (m_listenSock == null)
                    return;
                if (m_listenSock.Poll(10, SelectMode.SelectRead))
                {
                    Socket newClient = m_listenSock.Accept();
                    Session session = m_sessionManager.AddSession(newClient);
                    Console.WriteLine("Client" + session.id + "(" + session.ip + ")" + " has come");
                }
            }
        }

        private void ProcessReadableSession()
        {
            List<Session> readableSessions;
            readableSessions = m_sessionManager.GetReadableSessions();

            foreach (Session session in readableSessions)
            {
                List<string> messages = null;
                bool ret = ReceiveMessage(session, out messages);

                if (!ret)
                    continue;
                
                foreach(string message in messages)
                    SendEcho(session, message);
            }

        }

        private void SendEcho(Session session, String message)
        {
            IPAddress ipAddress = session.ip;
            byte[] msg = Encoding.UTF8.GetBytes(message + "<eom>");

            try
            {
                session.socket.Send(msg);
            }
            catch (SocketException)
            {
                m_sessionManager.RemoveSession(session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                m_sessionManager.RemoveSession(session);
            }
        }

        private bool ReceiveMessage(Session session, out List<string> messages)
        {
            Socket socket = session.socket;
            IPAddress ipAddress = session.ip;
            byte[] buf = new byte[1024];

            messages = new List<string>();

            string data = "";
            if (session.leftData != null)
            {
                data = session.leftData;
                session.leftData = null;
            }

            int bytesRec = 0;

            try
            {
                bytesRec = socket.Receive(buf);
            }
            catch (SocketException)
            {
                m_sessionManager.RemoveSession(session);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                m_sessionManager.RemoveSession(session);
                return false;
            }

            data += Encoding.UTF8.GetString(buf, 0, bytesRec);

            int index = -1;
            while((index = data.IndexOf("<eom>")) > -1)                 //If data has end of message,
            {
                string messageFragment = data.Substring(0, index);
                    
                messages.Add(messageFragment);                          //insert data fragment
                Console.WriteLine("Data from Client" + session.id + " : " + messageFragment);

                int lengthOfEOM = 5;
                int lengthOfRealData = index + 1;

                if (data.Length > lengthOfRealData + lengthOfEOM)       //If more data has come, 
                    data = data.Substring(index + lengthOfEOM);         //erase data stored in messages.
                else
                    data = "";
            }

            if(data.Length > 0)                                         //If data doesn't have any EOM, store the data in session.
            {
                session.leftData = data;
            }
            return true;
        }
    }
}
