using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class ChatRoom
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public bool IsPublic { get; init; }
        public uint UsersCount { get; set; } = 1;

        public ChatRoom(string id, string name, bool isPublic)
        {
            Id = id;
            Name = name;
            IsPublic = isPublic;
        }
    };
}