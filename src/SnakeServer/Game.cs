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
        public bool Full { get { return Count >= Config.PlayerCount; } }
        public bool Empty { get { return Count == 0; } }
        public Player[] Players { get { return _players.Values.ToArray(); } }
        public ServerState State { get; set; } = ServerState.Waiting;

        public event EventHandler Started;
        public event EventHandler Stopped;

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private Queue<Player> _joining = new Queue<Player>();
        private Queue<Player> _leaving = new Queue<Player>();

        private Queue<Apple> _spawningApples = new Queue<Apple>();
        private HashSet<Apple> _apples = new HashSet<Apple>();
        private Queue<Apple> _despawningApples = new Queue<Apple>();

        private Random _random = new Random();
        private double _width = Config.Width;
        private double _height = Config.Height;
        private int _playerCount = 4;

        private Timer _collisionTimer;
        private int _collisionCheckRate = 30;
        private bool _checkingCollision = false;

        private Timer _broadcastTimer;
        private int _broadcastRate = 2;

        private Timer _appleSpawnTimer;

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
            _appleSpawnTimer?.Dispose();
            _broadcastTimer?.Dispose();

            Stopped?.Invoke(this, null);
        }
        private void Start()
        {
            State = ServerState.Running;
            foreach (var player in _players.Values)
                player.Start(new Node(_random.Next((int)_width), _random.Next((int)_height)), _random.NextDouble() * 2 * Math.PI);

            _collisionTimer = new Timer(new TimerCallback(gameLoop), null, 1, 1000 / _collisionCheckRate);
            _appleSpawnTimer = new Timer(new TimerCallback(_spawnApple), null, 0, Config.AppleSpawnTime * 1000);
            _broadcastTimer = new Timer(new TimerCallback(_broadcastGameData), null, 0, 1000 / _broadcastRate);
            Started?.Invoke(this, null);
        }

        private void _broadcastGameData(object state)
        {
            List<SnakeModel> models = new List<SnakeModel>();
            var players = _players.Values.ToArray();
            foreach (var player in players)
                models.Add(new SnakeModel() { Nodes = player.Nodes, Heading = player.Heading, Length = player.Length, ID = player.ID });
            var model = new GameDataModel() { Snakes = models };
            _broadcast(model);
        }

        private void _spawnApple(object state)
        {
            var a = new Apple(_random.Next((int)_width), _random.Next((int)_height));
            a.Despawned += _despawnApple;
            _spawningApples.Enqueue(a);

            var model = new AppleSpawnedResponseModel(a);
            _broadcast(model);
        }
        private void _despawnApple(object sender, EventArgs e)
        {
            _despawningApples.Enqueue((Apple)sender);

            var model = new AppleDespawnedResponseModel(((Apple)sender).ID);
            _broadcast(model);
        }
        private void _broadcast(GameDataModel model)
        {
            foreach (var player in _players.Values.ToArray())
                Task.Run(() => player.Send(model));
        }
        private void _broadcast(ResponseModel model)
        {
            foreach (var player in _players.Values.ToArray())
                Task.Run(() => player.Send(model));
        }
        private void _playerDied(object sender, EventArgs e)
        {
            Console.WriteLine("Player died");
            ((Player)sender).Died -= _playerDied;
            _leaving.Enqueue((Player)sender);
            _broadcast(new SnakeDiedResponseModel(((Player)sender).ID));
            if (Empty)
                Stop();
        }
        private void _addSpawningApples()
        {
            int count = _spawningApples.Count;
            for (int i = 0; i < count; i++)
            {
                var apple = _spawningApples.Dequeue();
                _apples.Add(apple);
            }
        }
        private void _removeDespawningApples()
        {
            int count = _despawningApples.Count;
            for (int i = 0; i < count; i++)
            {
                var apple = _despawningApples.Dequeue();
                _apples.Remove(apple);
            }
        }
        private void _addJoiningPlayers()
        {
            int count = _joining.Count;
            for (int i = 0; i < count; i++)
            {
                var pl = _joining.Dequeue();
                _players.Add(pl.ID, pl);
            }
        }
        private void _removeLeavingPlayers()
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
            if (_checkingCollision)
                return;
            
            _checkingCollision = true;

            _addJoiningPlayers();

            var players = Players;
            
            foreach(var p1 in players)
            {
                var head1 = p1.Head;

                #region Check if player is inside the game field
                if(head1.X > _width || head1.X < 0 || head1.Y > _height || head1.Y < 0)
                {
                    p1.Die();
                    continue;
                }
                #endregion
                #region Check collision with other snakes
                foreach (var p2 in players)
                {
                    if (ReferenceEquals(p1, p2))
                        continue;

                    var nodes = p2.Nodes;
                    foreach(var node in nodes)
                    {
                        if(node?.DistanceTo(head1) < 2 * Config.SnakeRadius)
                        {
                            p1.Die();
                            break;
                        }
                    }
                    if (p1.State == Player.SnakeState.Dead)
                        break;
                }
                #endregion
                #region Check collision with apples
                _addSpawningApples();
                foreach (var apple in _apples)
                {
                    if (head1.DistanceTo(apple) < Config.SnakeRadius + Config.AppleRadius)
                    {
                        apple.Despawn();
                        p1.Grow(Config.AppleGrowLength);
                    }
                }
                _removeDespawningApples();
                #endregion
            }

            _removeLeavingPlayers();

            _checkingCollision = false;
        }
        public enum ServerState
        {
            Waiting,
            Running,
            Stopped
        }
    }
}