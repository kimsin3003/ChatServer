using System.Net;
using System.Net.Sockets;

namespace ChatServer
{

    class Session
    {
        private Socket socket;
        private IPAddress ip;


        private char[] id;
        public int sessionId;
        public int roomNo;
        public bool isConnected;

        public char[] Id
        {
            get { return id; }
        }

        public IPAddress Ip
        {
            get { return ip; }
        }

        public Socket Socket
        {
            get { return socket; }
        }

        public Session()
        {
            socket = null;
            ip = null;
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
        }

        public void Init(Socket socket)
        {
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
            this.socket = socket;
            id = null;
            ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
        }


        public Session(Session session)
        {
            socket = session.socket;
            id = session.id;
            sessionId = session.sessionId;
            ip = new IPAddress(session.ip.Address);
            isConnected = session.isConnected;
        }

        public void LogIn(char[] id)
        {
            this.id = id;
        }

        public bool IsLogedIn()
        {
            return (id == null) ? false : true;
        }

        public void LogOut()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            id = null;
            isConnected = false;
        }
        
    }

}
