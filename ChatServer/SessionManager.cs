using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ChatServer
{
    class SessionManager
    {
        private List<Session> sessions;
        private List<KeyValuePair<int, Session>> a;
        private Queue<int> idCount;
        private int largestUsingId;

        public SessionManager()
        {
            sessions = new List<Session>();
            idCount = new Queue<int>();
            idCount.Enqueue(1);
            largestUsingId = 0;

        }

        public List<Session> GetReadableSessions()
        {
            List<Session> readableSessions = new List<Session>();
            List<Session> closedSessions = new List<Session>();
            List<Session> sessionsClone;

            //prevent sessions list edited by accepting process while this function is editing
            lock (sessions)
            {
                sessionsClone = new List<Session>(sessions);
            }
            
            foreach (Session session in sessionsClone)
            {
                Socket socket = session.socket;
                
                //Although it's not listening socket
                if (socket.Poll(10, SelectMode.SelectRead)) //It has something to read,
                {
                    readableSessions.Add(session);
                }
            }

            return readableSessions;
        }

        public Session AddSession(Socket socket)
        {
            Session newSession = new Session();

            newSession.socket = socket;
            lock (sessions)
            {
                int count = idCount.Dequeue();
                newSession.sessionId = count;

                if(count > largestUsingId)
                {
                    idCount.Enqueue(count + 1);
                    largestUsingId = count;
                }

                newSession.ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
                sessions.Add(newSession);
            }

            return newSession;
        }

        public void RemoveSession(Session session)
        {
            lock(sessions)
            {
                Console.WriteLine("Client" + session.sessionId + "(" + session.ip + ") has exit");
                idCount.Enqueue(session.sessionId);
                session.socket.Shutdown(SocketShutdown.Both);
                session.socket.Close();
                sessions.Remove(session);
                session = null;
            }
        }
    }
}
