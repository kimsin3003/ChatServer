using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class CFSessionProcessor : SessionProcessor
    {
        public bool ProcessReadableSession(Session clientSession, Session backEndSession)
        {
            Socket socket = clientSession.Socket;
            IPAddress ipAddress = clientSession.Ip;

            CFHeader header;
            byte[] headerByte;
            byte[] body;
            int bodyLength;

            if (socket.Available == 0)
            {
                clientSession.isConnected = false;
                return false;
            }
            else
            {
                if (!ReceiveData(clientSession, out headerByte, Marshal.SizeOf(typeof(CFHeader))))
                {
                    return false;
                }
                header = (CFHeader)Serializer.ByteToStructure(headerByte, typeof(CFHeader));
                bodyLength = header.length;
            }

            if (!ReceiveData(clientSession, out body, bodyLength))
            {
                return false;
            }

            ProcessMessage(clientSession, backEndSession, header, body);

            return true;
        }

        private void ProcessMessage(Session session, Session backEndSession, CFHeader header, byte[] body)
        {
            CFMessageType type = header.type;
            int bodyLength = header.length;

            switch (type)
            {
                case CFMessageType.Id_Dup:
                case CFMessageType.Signup:
                case CFMessageType.Login:
                case CFMessageType.LogOut:
                    LoginMessage(session, backEndSession, header, body);
                    break;
                case CFMessageType.Room_Create:
                case CFMessageType.Room_Join:
                case CFMessageType.Room_Leave:
                case CFMessageType.Room_List:
                    RoomMessage(session, backEndSession, header, body);
                    break;
                case CFMessageType.Chat_MSG_From_Client:
                case CFMessageType.Chat_MSG_Broadcast:
                    ChatMassage(session, backEndSession, header, body);
                    break;
                default:
                    Console.WriteLine("Undefined Message Type");
                    break;
            }
        }

        private void LoginMessage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Id_Dup:
                    {
                        requestHeader.type = FBMessageType.Id_Dup;
                        break;
                    }
                case CFMessageType.Signup:
                    {
                        requestHeader.type = FBMessageType.Signup;
                        break;
                    }
                case CFMessageType.Login:
                    {
                        requestHeader.type = FBMessageType.Login;
                        break;
                    }
                case CFMessageType.LogOut:
                    {
                        requestHeader.type = FBMessageType.LogOut;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Login Message Type");
                    return;
            }

            requestHeader.length = Marshal.SizeOf<FBLoginRequestBody>();
            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;

            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);


            byte[] data;
            ReceiveData(clientSession, out data, Marshal.SizeOf<CFLoginRequestBody>());
            CFLoginRequestBody requestFromClient = (CFLoginRequestBody)Serializer.ByteToStructure(data, typeof(CFLoginRequestBody));

            FBLoginRequestBody requestBody = new FBLoginRequestBody();
            requestBody.id = requestFromClient.id;
            requestBody.password = requestFromClient.password;

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(backEndSession, bodyByte);
        }


        private void RoomMessage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {
            if (!clientSession.IsLogedIn())
            {
                CFHeader responseHeader = new CFHeader();
                responseHeader.type = header.type;
                responseHeader.state = CFMessageState.Fail;

                responseHeader.length = Marshal.SizeOf<CFRoomResponseBody>();

                byte[] failHeaderByte = Serializer.StructureToByte(responseHeader);
                SendData(clientSession, failHeaderByte);


                CFRoomResponseBody failResponseBody = new CFRoomResponseBody();

                byte[] failBodyByte = Serializer.StructureToByte(failResponseBody);

                SendData(clientSession, failBodyByte);
                return;
            }

            byte[] data;
            ReceiveData(clientSession, out data, Marshal.SizeOf<CFLoginRequestBody>());
            CFRoomRequestBody requestFromClient = (CFRoomRequestBody)Serializer.ByteToStructure(data, typeof(CFRoomRequestBody));

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Room_Create:
                    {
                        requestHeader.type = FBMessageType.Room_Create;
                        break;
                    }
                case CFMessageType.Room_Join:
                    {
                        requestHeader.type = FBMessageType.Room_Create;
                        break;
                    }
                case CFMessageType.Room_Leave:
                    {
                        requestHeader.type = FBMessageType.Room_Create;
                        break;
                    }
                case CFMessageType.Room_List:
                    {
                        requestHeader.type = FBMessageType.Room_Create;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Room Message Type");
                    return;
            }
            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;
            requestHeader.length = Marshal.SizeOf<FBRoomRequestBody>();
            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);



            FBRoomRequestBody requestBody = new FBRoomRequestBody();
            requestBody.id = requestFromClient.id;
            requestBody.roomNo = requestFromClient.roomNo;

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(backEndSession, bodyByte);
        }

        private void ChatMassage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {
            if (!clientSession.IsInRoom())
            {
                CFHeader responseHeader = new CFHeader();
                responseHeader.type = header.type;
                responseHeader.state = CFMessageState.Fail;

                responseHeader.length = Marshal.SizeOf<CFChatBody>();

                byte[] failHeaderByte = Serializer.StructureToByte(responseHeader);
                SendData(clientSession, failHeaderByte);


                CFChatBody failResponseBody = new CFChatBody();

                byte[] failBodyByte = Serializer.StructureToByte(failResponseBody);

                SendData(clientSession, failBodyByte);
                return;
            }

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Chat_MSG_From_Client:
                    {
                        SendBroadCast(clientSession.roomNo, body);
                        requestHeader.type = FBMessageType.Chat_Count;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Chat Message Type");
                    return;
            }

            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;
            requestHeader.length = Marshal.SizeOf<FBChatRequestBody>();
            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);

            byte[] data;
            ReceiveData(clientSession, out data, Marshal.SizeOf<CFLoginRequestBody>());

            //no use for now
            //CFLoginRequestBody requestFromClient = (CFLoginRequestBody)Serializer.ByteToStructure(data, typeof(CFLoginRequestBody));
            
            FBChatRequestBody requestBody = new FBChatRequestBody();

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(backEndSession, bodyByte);
        }

        private void SendBroadCast(int roomNo, byte[] message)
        {
            List<Session> users = RoomManager.GetInstance().GetUsersInRoom(roomNo);
            CFHeader header = new CFHeader();
            header.length = message.Length;
            header.type = CFMessageType.Chat_MSG_Broadcast;
            header.state = CFMessageState.Request;

            byte[] headerByte = Serializer.StructureToByte(header);

            foreach(var user in users)
            {
                SendData(user, headerByte);
                SendData(user, message);
            }
        }
    }

}
