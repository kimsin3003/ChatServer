using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{

    class RoomManager
    {
        private class Room
        {
            public List<Session> chatters;
            public int roomNo;

            public Room(int roomNo)
            {
                this.roomNo = roomNo;
                chatters = new List<Session>();
            }
        }

        private static RoomManager instance = null;
        private IDictionary<int, Room> rooms;
        private Queue<int> roomNumCount;

        private RoomManager()
        {
            rooms = new Dictionary<int, Room>();
            roomNumCount = new Queue<int>();
            roomNumCount.Enqueue(0);
        }
        
        public static RoomManager GetInstance()
        {
            if(instance == null)
            {
                instance = new RoomManager();
            }

            return instance;
        }

        
        public int MakeNewRoom()
        {
            int roomNo = roomNumCount.Dequeue();
            rooms.Add(roomNo, new Room(roomNo));
            Console.WriteLine("Room " + roomNo + " is made");

            return roomNo;
        }

        public void RemoveRoom(int roomNo)
        {
            rooms.Remove(roomNo);
            Console.WriteLine("Room " + roomNo + " is removed");
        }

        public List<Session> GetUsersInRoom(int roomNo)
        {
            Room room = rooms[roomNo];
            if(room == null)
            {
                Console.WriteLine("Room " + roomNo + " doesn't exits.");
            }

            return room.chatters;
        }

        public void AddUserInRoom(Session userSession, int roomNo)
        {
            Console.WriteLine(userSession.Id + " entered the room " + roomNo);
            rooms[roomNo].chatters.Add(userSession);
            userSession.roomNo = roomNo;
        }


        public void RemoveUserInRoom(Session userSession, int roomNo)
        {
            Console.WriteLine(userSession.Id + " went out the room " + roomNo);
            rooms[roomNo].chatters.Remove(userSession);
            userSession.roomNo = -1;
        }

        static public void ShutDown()
        {
            instance = null;            
        }
    }
}
