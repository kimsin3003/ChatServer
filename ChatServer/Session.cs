using System.Net;
using System.Net.Sockets;

namespace ChatServer
{

    class Session
    {
        public Socket socket;
        public int id;
        public IPAddress ip;
        public Session()
        {
            socket = null;
            id = -1;
            ip = null;
        }

        public Session(Session session)
        {
            socket = session.socket;
            id = session.id;
            ip = new IPAddress(session.ip.Address);
        }
    }

}
