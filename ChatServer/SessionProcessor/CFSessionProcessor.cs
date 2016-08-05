using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class CFSessionProcessor : SessionProcessor
    {
        public void ProcessTimeoutSession(Session clientSession, Session backEndSession)
        {
            if (!clientSession.isHealthCheckSent)
            {
                CFHeader header = new CFHeader();

                header.type = CFMessageType.Health_Check;
                header.state = CFMessageState.Request;
                header.length = 0;

                byte[] headerByte = Serializer.StructureToByte(header);
                SendData(clientSession, headerByte);
                clientSession.ResetStartTime();
                clientSession.isHealthCheckSent = true;
            }
            else
            {
                ConnectionCloseLogout(clientSession, backEndSession);
                clientSession.isConnected = false;
            }
        }

        private void ConnectionCloseLogout(Session clientSession, Session backEndSession)
        {
            CFHeader fakeHeader = new CFHeader();
            fakeHeader.type = CFMessageType.LogOut;
            fakeHeader.state = CFMessageState.Request;
            fakeHeader.length = Marshal.SizeOf(typeof(CFLoginRequestBody));

            CFLoginRequestBody fakeBody = new CFLoginRequestBody();
            fakeBody.id = clientSession.Id;

            byte[] fakeBodyByte = Serializer.StructureToByte(fakeBody);

            LoginMessage(clientSession, backEndSession, fakeHeader, fakeBodyByte);
        }

        public bool ProcessReadableSession(Session clientSession, Session backEndSession)
        {
            try
            {

                if (clientSession.Socket.Available == 0)
                {
                    if (clientSession.IsLogedIn())
                    {
                        Console.WriteLine(new string(clientSession.Id) + " has unexpectedly exited.");
                        if(clientSession.IsInRoom())
                        {
                            RoomManager.GetInstance().RemoveUserInRoom(clientSession);
                        }
                        ConnectionCloseLogout(clientSession, backEndSession);
                    }

                    clientSession.LogOut();
                    clientSession.isConnected = false;

                    return false;
                }
            }
            catch(ObjectDisposedException)
            {
                Console.WriteLine("Socket already closed");
                clientSession.isConnected = false;
                return false;
            }

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

            if(bodyLength > 0)
            {
                if (!ReceiveData(clientSession, out body, bodyLength))
                {
                    return false;
                }
            }
            else
            {
                body = null;
            }

            ProcessMessage(clientSession, backEndSession, header, body);

            return true;
        }

        

        private void ProcessMessage(Session clientSession, Session backEndSession, CFHeader header, byte[] body)
        {
            CFMessageType type = header.type;
            int bodyLength = header.length;

            switch (type)
            {
                case CFMessageType.Signup:
                    SignUpMessage(clientSession, backEndSession, header, body);
                    break;

                case CFMessageType.Id_Dup:
                case CFMessageType.Login:
                case CFMessageType.LogOut:
                    LoginMessage(clientSession, backEndSession, header, body);
                    break;

                case CFMessageType.Room_Create:
                case CFMessageType.Room_Join:
                case CFMessageType.Room_Leave:
                case CFMessageType.Room_List:
                    RoomMessage(clientSession, backEndSession, header, body);
                    break;

                case CFMessageType.Chat_MSG_From_Client:
                case CFMessageType.Chat_MSG_Broadcast:
                    ChatMassage(clientSession, backEndSession, header, body);
                    break;

                case CFMessageType.Health_Check:
                    {
                        clientSession.ResetStartTime();
                        clientSession.healthCheckCount = 0;
                        clientSession.isHealthCheckSent = false;
                        break;
                    }

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
                        Console.WriteLine("Login Request");
                        requestHeader.type = FBMessageType.Login;
                        break;
                    }
                case CFMessageType.LogOut:
                    {
                        Console.WriteLine("Logout Request");
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
                        SendBroadCast(CFMessageType.Room_Leave, clientSession.Id, clientSession.roomNo, null);
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
            requestHeader.length = Marshal.SizeOf(typeof(FBRoomRequestBody));
            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);


            if (body == null)
                return;
            FBRoomRequestBody requestBody = new FBRoomRequestBody();
            requestBody.id = clientSession.Id;
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
                responseHeader.length = 0;

                byte[] failHeaderByte = Serializer.StructureToByte(responseHeader);
                SendData(clientSession, failHeaderByte);
                return;
            }

            FBHeader requestHeader = new FBHeader();
            switch (header.type)
            {
                case CFMessageType.Chat_MSG_From_Client:
                    {
                        Console.WriteLine("Message Came");
                        if (body != null)
                        {
                            Console.WriteLine("Broadcasting");
                            SendBroadCast(CFMessageType.Chat_MSG_Broadcast, clientSession.Id, clientSession.roomNo, body);
                        }
                        requestHeader.type = FBMessageType.Chat_Count;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Chat Message Type");
                    return;
            }

            requestHeader.state = FBMessageState.Request;
            requestHeader.sessionId = clientSession.sessionId;

            
            FBChatRequestBody requestBody = new FBChatRequestBody();

            requestBody.id = clientSession.Id;

            byte[] bodyByte = Serializer.StructureToByte(requestBody);
            requestHeader.length = bodyByte.Length;
            
            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(backEndSession, headerByte);
            
            SendData(backEndSession, bodyByte);
        }

        private void SendBroadCast(CFMessageType type, char[] id, int roomNo, byte[] message)
        {
            List<Session> users = RoomManager.GetInstance().GetUsersInRoom(roomNo);
            CFHeader header = new CFHeader();
            header.type = type;
            header.state = CFMessageState.Request;

            CFChatResponseBody body = new CFChatResponseBody();
            body.date = DateTime.Now;
            body.id = id;

            if(type == CFMessageType.Chat_MSG_Broadcast)
            {
                body.msgLen = message.Length;
            }
            else
            {
                body.msgLen = 0;
            }

            byte[] bodyByte = Serializer.StructureToByte(body);

            header.length = bodyByte.Length;


            byte[] headerByte = Serializer.StructureToByte(header);



            foreach (var user in users)
            {
                SendData(user, headerByte);
                SendData(user, bodyByte);

                if(body.msgLen > 0)
                    SendData(user, message);
            }
        }
    }

}
