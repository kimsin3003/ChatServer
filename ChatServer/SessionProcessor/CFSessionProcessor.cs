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
            
            if (!ReceiveData(clientSession, out headerByte, Marshal.SizeOf(typeof(CFHeader))))
            {
                return false;
            }
            header = (CFHeader)Serializer.ByteToStructure(headerByte, typeof(CFHeader));
            bodyLength = header.length;

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
                case CFMessageType.Signup:
                    SignUpMessage(session, backEndSession, header, body);
                    break;
                case CFMessageType.Id_Dup:
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
        private void SignUpMessage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {
            FBHeader requestHeader = new FBHeader();

            requestHeader.type = FBMessageType.Signup;
            requestHeader.length = Marshal.SizeOf<FBSignupRequestBody>();
            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;

            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);
            
            CFSignupRequestBody requestFromClient = (CFSignupRequestBody)Serializer.ByteToStructure(body, typeof(CFSignupRequestBody));

            FBSignupRequestBody requestBody = new FBSignupRequestBody();
            requestBody.id = requestFromClient.id;
            requestBody.password = requestFromClient.password;
            requestBody.isDummy = requestFromClient.isDummy;

            byte[] bodyByte = Serializer.StructureToByte(requestBody);

            SendData(backEndSession, bodyByte);
        }
        private void LoginMessage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Id_Dup:
                    {
                        Console.WriteLine("ID dup Request");
                        requestHeader.type = FBMessageType.Id_Dup;
                        break;
                    }
                case CFMessageType.Login:
                    {
                        Console.WriteLine("ID Login Request");
                        requestHeader.type = FBMessageType.Login;
                        break;
                    }
                case CFMessageType.LogOut:
                    {
                        Console.WriteLine("ID Logout Request");
                        requestHeader.type = FBMessageType.LogOut;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Login Message Type");
                    return;
            }

            requestHeader.length = body.Length;
            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;

            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);

            
            CFLoginRequestBody requestFromClient = (CFLoginRequestBody)Serializer.ByteToStructure(body, typeof(CFLoginRequestBody));

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
            
            CFRoomRequestBody requestFromClient = (CFRoomRequestBody)Serializer.ByteToStructure(body, typeof(CFRoomRequestBody));

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Room_Create:
                    {
                        Console.WriteLine("Room Create Request");
                        requestHeader.type = FBMessageType.Room_Create;
                        break;
                    }
                case CFMessageType.Room_Join:
                    {
                        Console.WriteLine("Room Join Request " + "room no:" + requestFromClient.roomNo);
                        requestHeader.type = FBMessageType.Room_Join;
                        break;
                    }
                case CFMessageType.Room_Leave:
                    {
                        Console.WriteLine("Room Leave Request");
                        requestHeader.type = FBMessageType.Room_Leave;
                        break;
                    }
                case CFMessageType.Room_List:
                    {
                        Console.WriteLine("Room List Request");
                        requestHeader.type = FBMessageType.Room_List;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Room Message Type");
                    return;
            }
            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;
            requestHeader.length = body.Length;
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

                responseHeader.length = body.Length;

                byte[] failHeaderByte = Serializer.StructureToByte(responseHeader);
                SendData(clientSession, failHeaderByte);
                
                SendData(clientSession, body);
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
            requestHeader.length = body.Length;
            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);
            
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
