using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ChatServer
{
    class FBSessionProcessor : SessionProcessor
    {

        public void ProcessTimeoutSession(Session backEndSession)
        {
            if(!backEndSession.isHealthCheckSent)
            {
                FBHeader header = new FBHeader();

                header.type = FBMessageType.Health_Check;
                header.state = FBMessageState.Request;
                header.length = 0;
                header.sessionId = backEndSession.sessionId;

                byte[] headerByte = Serializer.StructureToByte(header);
                SendData(backEndSession, headerByte);
                backEndSession.healthCheckCount = 0;
                backEndSession.ResetTimer();
            }
            else
            {
                backEndSession.isConnected = false;
            }
        }
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

            if (!ReceiveData(backEndSession, out headerByte, Marshal.SizeOf(typeof(FBHeader))))
            {
                backEndSession.LogOut();
                return false;
            }

            header = (FBHeader)Serializer.ByteToStructure(headerByte, typeof(FBHeader));
            bodyLength = header.length;


            if (!ReceiveData(backEndSession, out body, bodyLength))
            {
                backEndSession.LogOut();
                return false;
            }


            ProcessMessage(backEndSession, header, body);
            return true;
        }

        private void ProcessMessage(Session backEndSession, FBHeader header, byte[] body)
        {
            FBMessageType type = header.type;
            int bodyLength = header.length;

            switch (type)
            {
                case FBMessageType.Signup:
                    SignupMessage(header, body);
                    break;
                case FBMessageType.Id_Dup:
                case FBMessageType.Login:
                case FBMessageType.LogOut:
                    LoginMessage(header, body);
                    break;
                case FBMessageType.Room_Create:
                case FBMessageType.Room_Join:
                case FBMessageType.Room_Leave:
                case FBMessageType.Room_List:
                case FBMessageType.Room_Delete:
                    RoomMessage(header, body);
                    break;

                case FBMessageType.Health_Check:
                    backEndSession.ResetTimer();
                    backEndSession.isHealthCheckSent = false;
                    backEndSession.healthCheckCount = 0;
                    break;

                case FBMessageType.Connection_Info:
                    ConnectionInfo(backEndSession);
                    break;
                default:
                    Console.WriteLine("Undefined Message Type");
                    break;
            }
        }

        private void HealthCheck(Session backEndSession, FBHeader header)
        {

            if (header.state == FBMessageState.Request)
            {
                FBHeader requestHeader = new FBHeader();
                Console.WriteLine("Signup Success");
                requestHeader.state = FBMessageState.Success;
                requestHeader.sessionId = header.sessionId;
                requestHeader.type = FBMessageType.Health_Check;
                requestHeader.length = 0;


                byte[] headerByte = Serializer.StructureToByte(requestHeader);
                SendData(backEndSession, headerByte);
            }
            else
            {
                backEndSession.ResetTimer();
                return;
            }
        }

        private void ConnectionInfo(Session backEndSession)
         {
            char[] ip = new char[15];

            foreach(IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                string s = address.ToString();
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    Array.Copy(address.ToString().ToCharArray(), ip, address.ToString().Length);
                    break;
                }
            }

            FBHeader header = new FBHeader();
            header.type = FBMessageType.Connection_Info;
            header.state = FBMessageState.Success;

            FBConnectionInfo info = new FBConnectionInfo();
            info.ip = ip;
            info.port = SessionManager.GetInstance().GetServicePort();

            byte[] body = Serializer.StructureToByte(info);
            header.length = body.Length;
            header.sessionId = -1;

            byte[] headerByte = Serializer.StructureToByte(header);
            backEndSession.Socket.Send(headerByte);

            backEndSession.Socket.Send(body);
        }

        private void SignupMessage(FBHeader header, byte[] body)
        {

            Session clientSession = SessionManager.GetInstance().GetSession(header.sessionId);
            CFHeader requestHeader = new CFHeader();

            if (header.state == FBMessageState.Success)
            {
                Console.WriteLine("Signup Success");
                requestHeader.state = CFMessageState.Success;
            }
            else if (header.state == FBMessageState.Fail)
            {
                requestHeader.state = CFMessageState.Fail;
            }
            else
            {
                requestHeader.state = CFMessageState.Request;
            }

            requestHeader.type = CFMessageType.Signup;
            
            requestHeader.length = 0;

            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(clientSession, headerByte);
            
        }

        private void LoginMessage(FBHeader header, byte[] body)
        {
            Session clientSession = SessionManager.GetInstance().GetSession(header.sessionId);
            CFHeader responseHeader = new CFHeader();

            if (header.state == FBMessageState.Success)
            {
                responseHeader.state = CFMessageState.Success;
            }
            else if (header.state == FBMessageState.Fail)
            {
                responseHeader.state = CFMessageState.Fail;
            }
            else
            {
                responseHeader.state = CFMessageState.Request;
            }

            switch (header.type)
            {
                case FBMessageType.Id_Dup:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine("Id not duplicated");
                        }
                        responseHeader.type = CFMessageType.Id_Dup;
                        break;
                    }
                case FBMessageType.Login:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            FBLoginResponseBody responseFromBackEnd = (FBLoginResponseBody)Serializer.ByteToStructure(body, typeof(FBLoginResponseBody));
                            clientSession.LogIn(responseFromBackEnd.id);
                            Console.WriteLine(new string(responseFromBackEnd.id) + " is logged in");
                        }
                        responseHeader.type = CFMessageType.Login;
                        break;
                    }
                case FBMessageType.LogOut:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine(new string(clientSession.Id) + " is logged out");
                            clientSession.LogOut();
                            if(clientSession.IsInRoom())
                            {
                                RoomManager.GetInstance().RemoveUserInRoom(clientSession);
                            }
                        }
                        responseHeader.type = CFMessageType.LogOut;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Login Message Type");
                    return;

            }
            
            responseHeader.length = 0;

            byte[] headerByte = Serializer.StructureToByte(responseHeader);
            SendData(clientSession, headerByte);
            
        }


        private void RoomMessage(FBHeader header, byte[] body)
        {
            Session clientSession = SessionManager.GetInstance().GetSession(header.sessionId);
            CFHeader requestHeader = new CFHeader();

            if (header.state == FBMessageState.Success)
            {
                requestHeader.state = CFMessageState.Success;
            }
            else if (header.state == FBMessageState.Fail)
            {
                requestHeader.state = CFMessageState.Fail;
            }
            else
            {
                requestHeader.state = CFMessageState.Request;
            }

            switch (header.type)
            {
                case FBMessageType.Room_Create:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine("Room Create Success");
                            int roomNo = BitConverter.ToInt32(body, 0);
                            RoomManager.GetInstance().MakeNewRoom(roomNo);
                        }
                        requestHeader.type = CFMessageType.Room_Create;
                        break;
                    }
                case FBMessageType.Room_Delete:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine("Room Delete Success");
                            RoomManager.GetInstance().RemoveRoom(clientSession.roomNo);
                        }
                        requestHeader.type = CFMessageType.Room_Delete;
                        break;
                    }
                case FBMessageType.Room_Join:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine("Room Join Success");
                            int roomNo = BitConverter.ToInt32(body, 0);
                            RoomManager.GetInstance().AddUserInRoom(clientSession, roomNo);
                        }
                        requestHeader.type = CFMessageType.Room_Join;
                        break;
                    }
                case FBMessageType.Room_Leave:
                    {
                        if (header.state == FBMessageState.Success)
                        {
                            Console.WriteLine("Room Leave Success");
                            RoomManager.GetInstance().RemoveUserInRoom(clientSession);
                        }
                        requestHeader.type = CFMessageType.Room_Leave;
                        break;
                    }
                case FBMessageType.Room_List:
                    {
                        Console.WriteLine("Room List Success");
                        requestHeader.type = CFMessageType.Room_List;
                        break;
                    }
                default:
                    Console.WriteLine("Undefined Room Message Type");
                    return;

            }

            if (body == null)
            {
                requestHeader.length = 0;
            }
            else
            {
                requestHeader.length = body.Length;
            }


            byte[] headerByte = Serializer.StructureToByte(requestHeader);
            SendData(clientSession, headerByte);

            if (body == null)
                return;
            
            SendData(clientSession, body);
        }

    }
}
