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

        private Timer _checkTimer;

        private HashSet<Game> _runningGames = new HashSet<Game>();
        private List<Game> _servers = new List<Game>();
        private Queue<Player> _playerQueue = new Queue<Player>();
        public Server(int port) : base(port)
        {
            _checkTimer = new Timer(new TimerCallback(_checkForEmptySpots), null, 1000, 50);
        }
        protected override void onClientConnect(Connection conn)
        {
            var player = new Player(conn);
            player.ConnectRequested += _connectRequested;
        }
        private void _connectRequested(object sender, System.EventArgs e)
        {
            Player pl = ((Player)sender);
            _playerQueue.Enqueue(pl);
        }
        private void _checkForEmptySpots(object state)
        {
            while (_servers.Count != 0 && _servers[0].State != Game.ServerState.Waiting)
                _servers.RemoveAt(0);

            if (_servers.Count == 0)
            {
                var game = new Game();
                game.Started += Game_Started;
                game.Stopped += Game_Stopped;
                _servers.Add(game);
            }

            if (_playerQueue.Count > 0)
                _servers[0].AddPlayer(_playerQueue.Dequeue());
        }

        private void Game_Stopped(object sender, EventArgs e)
        {
            _runningGames.Remove((Game)sender);
        }

        private void Game_Started(object sender, EventArgs e)
        {
            _runningGames.Add((Game)sender);
            _servers.Remove((Game)sender);
        }
    }
}