using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace backend.Hubs
{
    public class ChatHub(IMemoryCache _cache, IRoomsService _roomsService) : Hub<IChatClient>
    {
        public async Task<ChatRoom> CreateChat(string username, string chatName, bool isPublic)
        {
            //Генерируем уникальный ID чата
            string chatId = Guid.NewGuid().ToString();

            //Подключение в группу
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

            //Создаём объекты
            var room = new ChatRoom(chatId, chatName, isPublic);
            var userConnection = new UserConnection(username, chatId);

            //Записываем UserConnection в кэш
            var stringConnection = JsonConvert.SerializeObject(userConnection);
            _cache.Set(Context.ConnectionId, stringConnection);

            //Сохраняем комнату
            _roomsService.AddRoom(room);

            //Сообщение в успешном создании чата
            await Clients.Group(chatId)
                .ReceiveMessage("", $"Чат \"{room.Name}\" создан!");

            return room;
        }
        public async Task<ChatRoom?> JoinChatById(string username, string roomId)
        {
            var room = _roomsService.FindRoom(roomId);

            //Если такая комната существует
            if (room != null)
            {
                //Подключение в группу
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                //Записываем UserConnection в кэш
                var stringConnection = JsonConvert.SerializeObject(new UserConnection(username, roomId));
                _cache.Set(Context.ConnectionId, stringConnection);

                //Уведомляем сервис, что кто-то присоединился к чату
                _roomsService.OnJoinRoom(roomId);

                //Уведомляем остальных, что пользователь присоединился к чату
                await Clients.Group(roomId)
                        .ReceiveMessage("", $"Приветствуем {username} в чате!");
            }

            return room;
        }

        public async Task<ChatRoom?> JoinRandomChat(string username)
        {
            var room = _roomsService.GetRandomRoom();

            //Если есть хотя бы одна свободная публичная комната
            if (room != null)
            {
                //Подключемся к этому чату
                await JoinChatById(username, room.Id);
            }

            return room;
        }

        public async Task SendMessage(string message)
        {
            //Достаём из кэша подключение
            string connectionString = _cache.Get<string>(Context.ConnectionId)!;
            var connection = JsonConvert.DeserializeObject<UserConnection>(connectionString!);

            if (connection != null)
            {
                //Отправляем сообщение
                await Clients.Group(connection.RoomId)
                    .ReceiveMessage(connection.Username, message);
            }
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //Достаём из кэша подключение
            string connectionString = _cache.Get<string>(Context.ConnectionId)!;
            var connection = JsonConvert.DeserializeObject<UserConnection>(connectionString);

            if (connection != null)
            {

                //Удаляем подключение из кэша
                _cache.Remove(Context.ConnectionId);
                //Отключаемся от группы
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.RoomId);

                //Уведомляем сервис, что кто-то покинул чат
                _roomsService.OnLeaveRoom(connection.RoomId);

                //Уведомляем остальных, что пользователь покинул чат
                await Clients.Group(connection.RoomId)
                    .ReceiveMessage("", $"{connection.Username} покинул чат!");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}