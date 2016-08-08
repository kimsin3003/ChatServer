using System;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{

    class Session
    {
        public Socket socket
        {
            get;
            private set;
        }
        public IPAddress ip
        {
            get;
            private set;
        }


        public char[] id
        {
            get;
            private set;
        }
        public DateTime lastStartTime
        {
            get;
            private set;
        }
        public int sessionId;
        public int roomNo;
        public bool isConnected;
        
        public bool isHealthCheckSent;
        public int healthCheckCount;
        

        public void ResetStartTime()
        {
            lastStartTime = DateTime.Now;
        }

        public Session()
        {
            lastStartTime = default(DateTime);
            socket = null;
            ip = null;
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
            isHealthCheckSent = false;
            healthCheckCount = 0;
        }

        public void Init(Socket socket)
        {
            isConnected = false;
            sessionId = -1;
            roomNo = -1;
            this.socket = socket;
            id = null;
            lastStartTime = DateTime.Now;
            isHealthCheckSent = false;
            healthCheckCount = 0;
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
            id = null;
        }

        public bool IsInRoom()
        {
            return (roomNo == -1) ? false : true;
        }
        
    }

}
