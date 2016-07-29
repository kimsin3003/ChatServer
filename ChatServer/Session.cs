using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{

    class Session
    {
        public Socket socket;
        public int sessionId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] id;
        public IPAddress ip;
        public Session()
        {
            socket = null;
            sessionId = -1;
            ip = null;
        }

        public Session(Session session)
        {
            socket = session.socket;
            sessionId = session.sessionId;
            ip = new IPAddress(session.ip.Address);
        }
    }

}
