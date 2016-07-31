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
        private List<KeyValuePair<int, Session>> a;
        private Queue<int> idCount;
        static private SessionManager instance;

        private SessionManager()
        {
            connectedSessions = new Dictionary<int, Session>();

            idCount = new Queue<int>();
            idCount.Enqueue(0);

        }

        public void Init(int maxSessionNum)
        {
            sessionPool = new Queue<Session>(maxSessionNum);

            //set session pool
            for (int i = 0; i < maxSessionNum; i++)
            {
                sessionPool.Enqueue(new Session());
            }
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

        public List<Session> GetReadableSessions()
        {
            List<Session> readableSessions = new List<Session>();


            //prevent sessions list edited by accepting process while this function is editing
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
            return readableSessions;
        }

        public void RemoveClosedSessions()
        {
            lock (connectedSessions)
            {
                foreach (KeyValuePair<int, Session> item in connectedSessions)
                {
                    Session session = item.Value;
                    if(session.isConnected)
                    {
                        RemoveSession(session);
                    }
                }
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
        
        public void RemoveSession(Session session)
        {
            lock(connectedSessions)
            {
                if(connectedSessions.ContainsKey(session.sessionId))
                {
                    Console.WriteLine(session.Id +"(" + session.sessionId + ", " + session.Ip + ") has exit");

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
    }
}
