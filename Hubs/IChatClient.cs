using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Hubs
{
    public interface IChatClient
    {
        public Task ReceiveMessage(string username, string message);
    }
}