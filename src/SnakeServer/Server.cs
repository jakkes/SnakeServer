using Jakkes.WebSockets.Server;
using SnakeServer.GameObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;

namespace SnakeServer
{
    public class Server : WebSocketServer
    {

        private Timer _checkTimer = new Timer(new TimerCallback(_checkForEmptySpots), null, 1000, 1000);

        private ConcurrentQueue<Game> _servers = new ConcurrentQueue<Game>();
        private ConcurrentQueue<Player> _playerQueue = new ConcurrentQueue<Player>();
        private Dictionary<string, Player> _playersJoining = new Dictionary<string, Player>();
        public Server(int port) : base(port)
        {

        }
        protected override void onClientConnect(Connection conn)
        {
            var player = new Player(conn);
            player.ConnectRequest += _connectRequested;
            _playersJoining.Add(player.ID, player);
        }
        private void _connectRequested(object sender, System.EventArgs e)
        {
            Player pl = ((Player)sender);
            _playersJoining.Remove(pl.ID);
            _playerQueue.Enqueue(pl);
        }
        private static void _checkForEmptySpots(object state)
        {
            
        }
    }
}
