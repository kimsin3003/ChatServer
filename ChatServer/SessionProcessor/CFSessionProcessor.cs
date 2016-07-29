using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class CFSessionProcessor : SessionProcessor
    {
        public CFSessionProcessor(SessionManager sm) : base(sm)
        {
        }

        public override bool ProcessReadableSession(Session session)
        {
            Socket socket = session.socket;
            IPAddress ipAddress = session.ip;

            byte[] headerByte;
            byte[] body;
            int bodyLength;

            
            if (socket.Available == 0)
            {
                sessionManager.RemoveSession(session);
                return false;
            }
            else
            {
                if (!ReceiveData(session, out headerByte, Marshal.SizeOf(typeof(CFHeader))))
                {
                    return false;
                }
                CFHeader header = (CFHeader)Serializer.ByteToStructure(headerByte, typeof(CFHeader));
                bodyLength = header.length;
            }

            if (!ReceiveData(session, out body, bodyLength))
            {
                return false;
            }

            return true;
        }

        private void ProcessMessage(CFHeader header, byte[] body)
        {
            CFMessageType type = header.type;
            int bodyLength = header.length;

            switch (type)
            {
                case CFMessageType.Id_Dup:
                case CFMessageType.Signup:
                case CFMessageType.Login:
                    LoginProcess();
                    break;
                case CFMessageType.Room_Create:

                    break;
                case CFMessageType.Room_Join:

                    break;
                case CFMessageType.Room_Leave:

                    break;
                case CFMessageType.Room_List:

                    break;
                case CFMessageType.Chat_MSG_From_Client:

                    break;
                case CFMessageType.Chat_MSG_Broadcast:

                    break;

            }
        }

        void LoginProcess()
        {

        }

    }
}
