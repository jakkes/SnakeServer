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
        private Queue<Game> _waitingGames = new Queue<Game>();
        private Queue<Player> _playerQueue = new Queue<Player>();
        public Server(int port) : base(port)
        {
            _checkTimer = new Timer(new TimerCallback(_checkForEmptySpots), null, 1000, 1000);
        }
        public new void Close(bool hardquit){
            foreach (var game in _waitingGames)
            {
                game.Stop();
            }
            foreach (var conn in Connections)
            {
                conn.Close(hardquit);
            }
            base.Close(hardquit);
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
            Console.WriteLine("Connect requested.");
        }
        private void _checkForEmptySpots(object state)
        {
            while (_playerQueue.Count > 0)
            {
                if (_waitingGames.Count == 0)
                {
                    var game = new Game();
                    _waitingGames.Enqueue(game);
                    game.Started += Game_Started;
                    game.Stopped += Game_Stopped;
                }

                _waitingGames.Peek().AddPlayer(_playerQueue.Dequeue());
                if (_waitingGames.Peek().Full)
                    _waitingGames.Dequeue();
            }
        }

        private void Game_Stopped(object sender, EventArgs e)
        {
            _runningGames.Remove((Game)sender);
            Console.WriteLine("Game stopped.");
        }

        private void Game_Started(object sender, EventArgs e)
        {
            _runningGames.Add((Game)sender);
            Console.WriteLine("Started new game.");
        }
    }
}