using ASP_Projekt_WebChat_PiotrBudz.SocketsManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASP_Projekt_WebChat_PiotrBudz.Handlers
{
    public class WebSocketMessageHandler : SocketHandler
    {

        private Dictionary<string, string> nicknames = new Dictionary<string, string>();
        private object nicknamesLock = new object();
        //private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public WebSocketMessageHandler(ConnectionManager connections) : base(connections)
        {

        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);
            var socketId = Connections.GetId(socket);
            //await SendMessageToAll($"{socketId} dołączył do czatu.");
        }

        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = Connections.GetId(socket);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            if (!nicknames.ContainsKey(socketId))
            {
                var delimiterIndex = message.IndexOf(':');
                if (delimiterIndex != -1)
                {
                    var nickname = message.Substring(0, delimiterIndex).Trim();
                    var userMessage = message.Substring(delimiterIndex + 1).Trim();

                    bool nicknameSet = await SetNickname(socket, nickname);
                }
            }
            else
            {
                var nickname = nicknames[socketId];
                var chatMessage = $"{nickname}: {message}";
                await SendMessageToAll(chatMessage);
            }
        }

        private bool TrySetNickname(string socketId, string nickname)
        {
            lock (nicknamesLock)
            {
                if (!nicknames.ContainsValue(nickname))
                {
                    nicknames[socketId] = nickname;
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> SetNickname(WebSocket socket, string nickname)
        {
            var socketId = Connections.GetId(socket);
            if (TrySetNickname(socketId, nickname))
            {
                await SendMessage(socket, $"Twoj nick to: {nickname}");
                await SendMessageToAll($"{nickname} dolaczyl do czatu.");
                return true;
            }
            else
            {
                await SendMessage(socket, $"Nick jest juz zajety, wybierz inny.");
                return false;
            }
        }
    }
}



//using ASP_Projekt_WebChat_PiotrBudz.SocketsManager;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace ASP_Projekt_WebChat_PiotrBudz.Handlers
//{
//    public class WebSocketMessageHandler : SocketHandler
//    {

//        private Dictionary<string, string> nicknames = new Dictionary<string, string>();
//        private object lockObject = new object();

//        public WebSocketMessageHandler(ConnectionManager connections) : base(connections)
//        {

//        }
//        public override async Task OnConnected(WebSocket socket)
//        {
//            await base.OnConnected(socket);
//            var socketId = Connections.GetId(socket);
//            await SendMessageToAll($"{socketId} dolaczyl do czatu.");
//        }
//        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
//        {
//            var socketId = Connections.GetId(socket);
//            var nickname = nicknames.ContainsKey(socketId) ? nicknames[socketId] : null;
//            var message = $"{GetDisplayName(socketId, nickname)}: {Encoding.UTF8.GetString(buffer, 0, result.Count)}";
//            await SendMessageToAll(message);
//        }

//        private string GetDisplayName(string socketId, string nickname)
//        {
//            if (!string.IsNullOrEmpty(nickname))
//                return nickname;
//            return null;
//        }



//    }
//}
