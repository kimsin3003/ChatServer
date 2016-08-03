using System;
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
        private DateTime startTime;
        private bool healthCheckSent;

        public char[] Id
        {
            get { return id; }
        }

        public HealthCheckSend()
        {

        }

        public IPAddress Ip
        {
            get { return ip; }
        }

        public Socket Socket
        {
            get { return socket; }
        }

        public DateTime Time
        {
            get { return startTime; }
        }

        public void ResetTimer()
        {
            startTime = DateTime.Now;
        }

        public Session()
        {
            startTime = default(DateTime);
            socket = null;
            ip = null;
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
        }

        public void Init(Socket socket, double healthCheckTimeLimit)
        {
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
            this.socket = socket;
            id = null;
            startTime = DateTime.Now;
            ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
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
        }

        public bool IsInRoom()
        {
            return (roomNo == -1) ? false : true;
        }
        
    }

}
