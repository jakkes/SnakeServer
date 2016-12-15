using Jakkes.WebSockets.Server;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SnakeServer.GameObjects;

namespace SnakeServer
{
    public class Game : WebSocketServer
    {
        ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();
        ConcurrentQueue<Player> _joining = new ConcurrentQueue<Player>();
        ConcurrentQueue<Player> _leaving = new ConcurrentQueue<Player>();

        public Game(int port) : base(port) { }
        protected override void onClientConnect(Connection conn)
        {
            var player = new Player(conn);
            if (!_players.TryAdd(player.ID, player))
                _joining.Enqueue(player);
        }
    }
}
