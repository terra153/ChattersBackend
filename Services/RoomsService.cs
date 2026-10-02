using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services
{
    public class RoomsService : IRoomsService
    {
        readonly private List<ChatRoom> _rooms = [];
        public void AddRoom(ChatRoom room)
        {
            _rooms.Add(room);
        }

        public ChatRoom? FindRoom(string id)
        {
            return _rooms.FirstOrDefault(r => r.Id == id);
        }

        public ChatRoom? GetRandomRoom()
        {
            //только публичные чаты
            var availableRooms = _rooms.Where(r => r.IsPublic == true);

            return availableRooms.FirstOrDefault();
        }

        public void OnJoinRoom(string id)
        {
            ChatRoom? room = _rooms.FirstOrDefault(r => r.Id == id);

            if (room == null) return;

            room.UsersCount++;
        }

        public void OnLeaveRoom(string id)
        {
            ChatRoom? room = _rooms.FirstOrDefault(r => r.Id == id);

            if (room == null) return;

            room.UsersCount--;

            //Автоматически удаляем комнату, если в ней никого нет
            if (room.UsersCount == 0)
                _rooms.Remove(room);
        }
    }
}