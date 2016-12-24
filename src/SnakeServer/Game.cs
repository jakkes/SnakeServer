using Jakkes.WebSockets.Server;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SnakeServer.GameObjects;
using System.Threading;
using Newtonsoft.Json;
using SnakeServer.Models;

namespace SnakeServer
{
    public class Game    {
        public int Count { get { return _players.Count + _joining.Count - _leaving.Count; } }
        public ServerState State { get; set; } = ServerState.Waiting;

        public event EventHandler Started;
        public event EventHandler Stopped;

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private Queue<Player> _joining = new Queue<Player>();
        private Queue<Player> _leaving = new Queue<Player>();

        private Random _random = new Random();
        private const int _width = 1024;
        private const int _height = 720;
        private const int _playerCount = 4;

        private Timer _collisionTimer;
        private int _collisionCheckRate = 60;
        
        private int _snakeRadius = 5;

        public Game() { }
        public void AddPlayer(Player player)
        {
            player.Died += _playerDied;
            _players.Add(player.ID, player);
            if (_players.Count >= _playerCount)
                Start();
        }
        public void Stop()
        {
            State = ServerState.Stopped;
            _collisionTimer?.Dispose();

            Stopped?.Invoke(this, null);
        }
        private void Start()
        {
            State = ServerState.Running;
            foreach (var player in _players.Values)
                player.Start(new Node(_random.Next(_width), _random.Next(_height)), _random.NextDouble() * 2 * Math.PI);

            _collisionTimer = new Timer(new TimerCallback(gameLoop), null, 0, 1000 / _collisionCheckRate);
            Started?.Invoke(this, null);
        }
        private void _playerDied(object sender, EventArgs e)
        {
            ((Player)sender).Died -= _playerDied;
            _leaving.Enqueue((Player)sender);
            if (_players.Count - _leaving.Count <= 0)
            {
                Stop();
            }
        }
        private void _addJoining()
        {
            int count = _joining.Count;
            for (int i = 0; i < count; i++)
            {
                var pl = _joining.Dequeue();
                _players.Add(pl.ID, pl);
            }
        }
        private void _removeLeaving()
        {
            int count = _leaving.Count;
            for (int i = 0; i < count; i++)
            {
                var pl = _leaving.Dequeue();
                _players.Remove(pl.ID);
            }
        }
        private void gameLoop(object state)
        {
            if (State == ServerState.Stopped)
                return;

            _addJoining();

            var players = _players.Values;
            
            foreach(var p1 in players)
            {
                var head1 = p1.Head;

                if(head1.X > _width || head1.X < 0 || head1.Y > _height || head1.Y < 0)
                {
                    p1.Die();
                    continue;
                }

                foreach (var p2 in players)
                {
                    if (ReferenceEquals(p1, p2))
                        continue;

                    var nodes = p2.Nodes;
                    foreach(var node in nodes)
                    {
                        if(node?.DistanceTo(head1) < 2 * _snakeRadius)
                        {
                            p1.Die();
                            break;
                        }
                    }
                    if (p1.State == Player.SnakeState.Dead)
                        break;
                }
            }

            _removeLeaving();

            List<SnakeModel> models = new List<SnakeModel>();
            foreach (var player in players)
                models.Add(new SnakeModel() { Nodes = player.Nodes, Heading = player.Heading });
            string serialized = JsonConvert.SerializeObject(new SnakeDataModel() { Snakes = models });
            foreach (var player in players)
                player.Send(serialized);
        }
        public enum ServerState
        {
            Waiting,
            Running,
            Stopped
        }
    }
}