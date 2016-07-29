using System;
using System.Net.Sockets;

namespace ChatServer
{
    abstract class SessionProcessor
    {
        protected SessionManager sessionManager;
        public SessionProcessor(SessionManager sm)
        {
            sessionManager = sm;
        }

        public abstract bool ProcessReadableSession(Session session);

        protected bool ReceiveData(Session session, out byte[] buf, int size)
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

        protected bool SendData(Session session, byte[] buf, int size)
        {
            try
            {
                session.socket.Send(buf);
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
