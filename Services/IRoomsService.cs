using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services
{
    public interface IRoomsService
    {
        public void AddRoom(ChatRoom room);
        public void OnLeaveRoom(string id);
        public void OnJoinRoom(string id);
        public ChatRoom? FindRoom(string id);
        public ChatRoom? GetRandomRoom();
    }
}