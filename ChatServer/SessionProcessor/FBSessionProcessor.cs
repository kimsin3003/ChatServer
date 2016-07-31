using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class FBSessionProcessor : SessionProcessor
    {

        public bool ProcessReadableSession(Session backEndSession)
        {
            Socket socket = backEndSession.Socket;
            IPAddress ipAddress = backEndSession.Ip;

            FBHeader header;
            byte[] headerByte;
            byte[] body;
            int bodyLength;


            if (socket.Available == 0)
            {
                backEndSession.isConnected = false;
                return false;
            }
            else
            {
                if (!ReceiveData(backEndSession, out headerByte, Marshal.SizeOf(typeof(FBHeader))))
                {
                    return false;
                }
                header = (FBHeader)Serializer.ByteToStructure(headerByte, typeof(FBHeader));
                bodyLength = header.length;

                if (!ReceiveData(backEndSession, out body, bodyLength))
                {
                    return false;
                }

                ProcessMessage(backEndSession, header, body);
                return true;
            }
        }

        private void ProcessMessage(Session backEndSession, FBHeader header, byte[] body)
        {
            FBMessageType type = header.type;
            int bodyLength = header.length;
            Session targetClientSession = SessionManager.GetInstance().GetSession(header.sessionId);

            switch (type)
            {
                case FBMessageType.Id_Dup:
                case FBMessageType.Signup:
                case FBMessageType.Login:
                case FBMessageType.LogOut:
                    LoginMessage(backEndSession, header, body);
                    break;
                case FBMessageType.Room_Create:
                case FBMessageType.Room_Join:
                case FBMessageType.Room_Leave:
                case FBMessageType.Room_List:
                case FBMessageType.Room_Delete:
                    RoomMessage(backEndSession, header, body);
                    break;
                default:
                    Console.WriteLine("Undefined Message Type");
                    break;
            }
        }

        private void LoginMessage(Session clientSession, FBHeader header, byte[] body)
        {

            CFHeader RequestHeader = new CFHeader();

            if (header.state == FBMessageState.Success)
            {
                RequestHeader.state = CFMessageState.Success;
            }
            if (header.state == FBMessageState.Request)
            {
                RequestHeader.state = CFMessageState.Request;
            }
            else if(header.state == FBMessageState.Fail)
            {
                RequestHeader.state = CFMessageState.Fail;
            }

            byte[] data;
            ReceiveData(clientSession, out data, Marshal.SizeOf<FBLoginResponseBody>());
            FBLoginResponseBody requestFromClient = (FBLoginResponseBody)Serializer.ByteToStructure(data, typeof(FBLoginResponseBody));
            switch (header.type)
            {
                case FBMessageType.Id_Dup:
                    {
                        RequestHeader.type = CFMessageType.Id_Dup;
                        break;
                    }
                case FBMessageType.Signup:
                    {
                        RequestHeader.type = CFMessageType.Signup;
                        break;
                    }
                case FBMessageType.Login:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            clientSession.LogIn(requestFromClient.id);
                        }
                        RequestHeader.type = CFMessageType.Login;
                        break;
                    }
                case FBMessageType.LogOut:
                    {
                        clientSession.LogOut();
                        RequestHeader.type = CFMessageType.LogOut;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Login Message Type");
                    return;

            }

            byte[] headerByte = Serializer.StructureToByte(RequestHeader);
            SendData(clientSession, headerByte);
            
            CFLoginResponseBody requestBody = new CFLoginResponseBody();

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(clientSession, bodyByte);
        }


        private void RoomMessage(Session clientSession, FBHeader header, byte[] body)
        {

            CFHeader RequestHeader = new CFHeader();

            if (header.state == FBMessageState.Success)
            {
                RequestHeader.state = CFMessageState.Success;
            }
            else if (header.state == FBMessageState.Fail)
            {
                RequestHeader.state = CFMessageState.Success;
            }
            else
            {
                RequestHeader.state = CFMessageState.Request;
            }

            switch (header.type)
            {
                case FBMessageType.Room_Create:
                    {
                        RequestHeader.type = CFMessageType.Room_Create;
                        break;
                    }
                case FBMessageType.Room_Delete:
                    {
                        RequestHeader.type = CFMessageType.Room_Delete;
                        break;
                    }
                case FBMessageType.Room_Join:
                    {
                        RequestHeader.type = CFMessageType.Room_Join;
                        break;
                    }
                case FBMessageType.Room_Leave:
                    {
                        RequestHeader.type = CFMessageType.Room_Leave;
                        break;
                    }
                case FBMessageType.Room_List:
                    {
                        RequestHeader.type = CFMessageType.Room_List;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Room Message Type");
                    return;

            }

            byte[] headerByte = Serializer.StructureToByte(RequestHeader);
            SendData(clientSession, headerByte);

            byte[] data;
            ReceiveData(clientSession, out data, Marshal.SizeOf<FBRoomResponseBody>());

            //No use for now.
            //FBLoginResponseBody requestFromClient = (FBLoginResponseBody)Serializer.ByteToStructure(data, typeof(FBLoginResponseBody));

            CFRoomResponseBody requestBody = new CFRoomResponseBody();

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(clientSession, bodyByte);
        }

    }
}
