using System;
using System.Net.Sockets;

namespace ChatServer
{
    abstract class SessionProcessor
    {
        protected bool ReceiveData(Session session, out byte[] buf, int size)
        {

            if(size == 0)
            {
                buf = null;
                return true;
            }

            buf = new byte[size];
            try
            {
                if(session.Socket.Receive(buf) == 0)
                {
                    return false;
                }
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        protected bool SendData(Session session, byte[] buf)
        {
            try
            {
                session.Socket.Send(buf);
            }
            catch (SocketException)
            {
                session.LogOut();
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                session.LogOut();
                return false;
            }
            return true;
        }

    }
}
