using Jakkes.WebSockets.Server;
using System.Collections.Generic;
using System;
using System.Threading;
using Newtonsoft.Json;
using SnakeServer.Models;
using System.Threading.Tasks;

namespace SnakeServer.GameObjects
{
    public class Player
    {
        private const int _moveRate = 60;
        private const int _moveLength = 1;

        private Connection _conn;
        private List<Node> _nodes;
        private Timer _moveTimer;
        private int _length = 6;

        public event EventHandler ConnectRequested;
        public event EventHandler Died;

        public string ID { get { return _conn.ID; } }
        public double Heading { get; private set; }
        public Node Head { get { return _nodes[0]; } }
        public Node[] Nodes { get { lock (_nodes) return _nodes.ToArray(); } }
        public SnakeState State { get; set; } = SnakeState.Alive;

        public Player(Connection conn)
        {
            _conn = conn;
            _conn.MessageReceived += _conn_MessageReceived;
            _conn.Closed += _conn_Closed;
            _conn.Send(JsonConvert.SerializeObject(new ConnectResponseModel() { Id = ID }));
        }
        public void Start(Node start, double heading)
        {
            Heading = heading;
            _nodes = new List<Node>();
            _nodes.Add(start);
            _moveTimer = new Timer(new TimerCallback(Move), null, 1000, 1000 / 60);
        }
        public void Send(string message)
        {
            _conn.Send(message);
        }
        public void Die()
        {
            State = SnakeState.Dead;
            Died?.Invoke(this, null);
        }
        private void Move(object state)
        {
            _nodes.Insert(0, new Node(Head.X + _moveLength * Math.Cos(Heading), Head.Y + _moveLength * Math.Sin(Heading)));
            if (_nodes.Count > _length)
                _nodes.RemoveRange(_length, _nodes.Count - _length);
        }
        private void _conn_Closed(Connection source)
        {
            Die();
        }
        private void _conn_MessageReceived(Connection source, string data)
        {
            RequestModel model = JsonConvert.DeserializeObject<RequestModel>(data);

            switch (model.Action)
            {
                case "Connect":
                    ConnectRequested?.Invoke(this, null);
                    break;
            }
        }
        public enum SnakeState
        {
            Alive,
            Dead
        }
    }
}
