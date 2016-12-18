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
    public class Game : WebSocketServer
    {
        ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();
        ConcurrentQueue<Player> _joining = new ConcurrentQueue<Player>();
        ConcurrentQueue<Player> _leaving = new ConcurrentQueue<Player>();

        private Random _random = new Random();
        private const int _width = 1000;
        private const int _height = 1000;

        private Timer _collisionTimer;
        private int _collisionCheckRate = 60;
        
        private int _snakeRadius = 5;

        public Game(int port) : base(port) { }
        protected override void onClientConnect(Connection conn)
        {
            var player = new Player(conn);
            player.Died += _playerDied;
            player.ConnectRequest += _connectRequest;

            if (!_players.TryAdd(player.ID, player))
                _joining.Enqueue(player);
        }
        private void _connectRequest(object sender, EventArgs e)
        {
            ((Player)sender).Start(new Node(_random.Next(_width),_random.Next(_height)), _random.NextDouble() * 2 * Math.PI);
        }
        private void _playerDied(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        public new void Start()
        {
            base.Start();
            _collisionTimer = new Timer(new TimerCallback(collisionCheck), null, 1000, 1000 / _collisionCheckRate);
        }
        private void collisionCheck(object state)
        {
            int count = _joining.Count;
            for (int i = 0; i < count; i++)
            {
                Player pl;
                if (_joining.TryDequeue(out pl))
                {
                    if (!_players.TryAdd(pl.ID, pl))
                        _joining.Enqueue(pl);
                }
            }

            var players = _players.Values;
            
            foreach(var p1 in players)
            {
                var head1 = p1.Head;

                foreach (var p2 in players)
                {
                    if (ReferenceEquals(p1, p2))
                        continue;

                    var nodes = p2.Nodes;
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (head1.DistanceTo(nodes[i]) < 2 * _snakeRadius)
                        {
                            p1.Die();
                            p2.Die();
                        }
                    }
                }
            }

            count = _leaving.Count;
            for (int i = 0; i < count; i++)
            {
                Player pl;
                if (_leaving.TryDequeue(out pl))
                {
                    if (!_players.TryRemove(pl.ID, out pl))
                        _leaving.Enqueue(pl);
                }
            }

            List<SnakeModel> models = new List<SnakeModel>();
            foreach (var player in players)
                models.Add(new SnakeModel() { Nodes = player.Nodes, Heading = player.Heading });
            Broadcast(JsonConvert.SerializeObject(models));
        }
    }
}