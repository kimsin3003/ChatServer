using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ChatServer
{
    class SessionManager
    {
        private IDictionary<int, Session> connectedSessions;
        private Queue<Session> sessionPool;
        private Queue<int> idCount;
        static private SessionManager instance = null;
        private int servicePort;


        private SessionManager()
        {
            connectedSessions = new Dictionary<int, Session>();

            idCount = new Queue<int>();
            idCount.Enqueue(0);

        }

        public void Init(int maxSessionNum, int port)
        {
            sessionPool = new Queue<Session>(maxSessionNum);
            servicePort = port;

            //set session pool
            for (int i = 0; i < maxSessionNum; i++)
            {
                sessionPool.Enqueue(new Session());
            }
        }

        public int GetServicePort()
        {
            return servicePort;
        }

        static public SessionManager GetInstance()
        {
            if(instance == null)
            {
                instance = new SessionManager();
            }

            return instance;
        }

        public Session GetSession(int sessionId)
        {
            if (!connectedSessions.ContainsKey(sessionId))
            {
                Console.WriteLine("sessionId " + sessionId + " doesn't exist");
                return null;
            }

            return connectedSessions[sessionId];
        }

        public List<Session> GetTimedoutSessions()
        {
            List<Session> timedOutSessions = new List<Session>();
            lock (connectedSessions)
            {
                foreach(KeyValuePair<int,Session> item in connectedSessions)
                {
                    Session session = item.Value;
                    if(session.isHealthCheckSent)
                    {
                        if ((DateTime.Now - session.LastStartTime).TotalSeconds >= 10)
                        {
                            timedOutSessions.Add(session);
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - session.LastStartTime).TotalSeconds >= 30)
                        {
                            timedOutSessions.Add(session);
                        }
                    }
                }
            }

            return timedOutSessions;
        }

        public List<Session> GetReadableSessions()
        {
            List<Session> readableSessions = new List<Session>();
            List<Socket> sockets = new List<Socket>();
            lock (connectedSessions)
            {
                foreach (KeyValuePair<int, Session> item in connectedSessions)
                {
                    sockets.Add(item.Value.Socket);
                }
            }

            try
            {
                Socket.Select(sockets, null, null, 1000000);
            }
            catch (SocketException)
            {
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (sockets.Count > 0)
            {
                lock (connectedSessions)
                {
                    foreach (KeyValuePair<int, Session> item in connectedSessions)
                    {
                        Session session = item.Value;
                        Socket socket = session.Socket;

                        if (socket.Poll(10, SelectMode.SelectRead))
                        {
                            readableSessions.Add(session);
                        }
                    }
                }
            }

            return readableSessions;
        }

        public void RemoveClosedSessions()
        {
            List<Session> sessionToRemove = new List<Session>();
            
            lock (connectedSessions)
            {
                foreach (KeyValuePair<int, Session> item in connectedSessions)
                {
                    Session session = item.Value;
                    if(!session.isConnected)
                    {
                        sessionToRemove.Add(session);
                    }
                }
            }

            foreach (Session session in sessionToRemove)
            {
                RemoveSession(session);
            }

        }

        public Session MakeNewSession(Socket socket)
        {
            Session newSession = sessionPool.Dequeue();

            if(newSession == null)
            {
                Console.WriteLine("Session pool is empty!");
            }

            newSession.Init(socket);

            lock (connectedSessions)
            {
                int sessionId = idCount.Dequeue();

                if(!connectedSessions.ContainsKey(sessionId + 1))
                {
                    idCount.Enqueue(sessionId + 1);
                }

                newSession.sessionId = sessionId;
                newSession.isConnected = true;


                connectedSessions.Add(newSession.sessionId, newSession);
            }

            return newSession;
        }
        
        private void RemoveSession(Session session)
        {
            lock(connectedSessions)
            {
                if(connectedSessions.ContainsKey(session.sessionId))
                {
                    Console.WriteLine(new string(session.Id) +"(" + session.sessionId + ", " + session.Ip + ") has exit");

                    connectedSessions.Remove(session.sessionId);

                    idCount.Enqueue(session.sessionId);
                    session.sessionId = -1;
                    sessionPool.Enqueue(session);
                }
                else
                {
                    Console.WriteLine("session " + session.sessionId + " doesn't exist");
                }
            }
        }

        static public void ShutDown()
        {
            instance = null;
        }
    }
}
