using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class FBSessionProcessor : SessionProcessor
    {
        Socket backEndSocket;
        public FBSessionProcessor(SessionManager sessionManager) : base(sessionManager)
        {
        }

        public override void ProcessMessage(Header header, byte[] bodyByte)
        {
            throw new NotImplementedException();
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
                FBHeader header = (FBHeader)Serializer.ByteToStructure(headerByte, typeof(FBHeader));
                bodyLength = header.length;

                if (!ReceiveData(session, out body, bodyLength))
                {
                    return false;
                }
                

                return true;
            }
        }
    }
}
