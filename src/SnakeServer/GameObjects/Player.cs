using Jakkes.WebSockets.Server;
using System.Collections.Generic;
using System;
using System.Threading;
using Newtonsoft.Json;
using SnakeServer.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SnakeServer.GameObjects
{
    public class Player
    {

        private Connection _conn;
        private List<Node> _nodes;
        private Timer _moveTimer;
        private Timer _turnTimer;

        private int _length = Config.SnakeStartLength;

        public event EventHandler ConnectRequested;
        public event EventHandler Died;

        
        public string ID { get { return _conn.ID; } }
        public double Heading { get; private set; }
        public Node Head { get { return _nodes[0]; } }
        public Node[] Nodes { get { lock (_nodes) return _nodes.ToArray(); } }
        public SnakeState State { get; set; } = SnakeState.Dead;
        public TurnState Turn
        {
            get
            {
                return _turn;
            }
            set
            {
                _turnTimer?.Dispose();
                _turn = value;

                if (_turn != TurnState.None)
                    _turnTimer = new Timer(new TimerCallback(_turnElapsed), null, 0, (int)(1000 / Config.SnakeTurnRate));
            }
        }
        private TurnState _turn = TurnState.None;

        public Player(Connection conn)
        {
            _conn = conn;
            _conn.MessageReceived += _conn_MessageReceived;
            _conn.Closed += _conn_Closed;
            _conn.Send(JsonConvert.SerializeObject(new ConnectResponseModel() { Id = ID }));
        }
        public void Start(Node start, double heading)
        {
            State = SnakeState.Alive;
            Heading = heading;
            _nodes = new List<Node>();
            _nodes.Add(start);
            _moveTimer = new Timer(new TimerCallback(Move), null, 1000, 1000 / Config.SnakeMovementRate);
        }
        public void Send(string message)
        {
            try
            {
                _conn.Send(message);
            } catch (Exception)
            {
                Die();
            }
        }
        public void Die()
        {
            State = SnakeState.Dead;
            _moveTimer?.Dispose();
            _turnTimer?.Dispose();
            if (_conn.State == WebSocketState.Open)
                Send(JsonConvert.SerializeObject(new DiedResponseModel()));
            Died?.Invoke(this, null);
        }
        public void Grow(int length)
        {
            _length += length;
        }
        private void Move(object state)
        {
            _nodes.Insert(0, new Node(Head.X + Config.SnakeMovementLength * Math.Cos(Heading), Head.Y + Config.SnakeMovementLength * Math.Sin(Heading)));
            if (_nodes.Count > _length)
                _nodes.RemoveRange(_length, _nodes.Count - _length);
        }
        private void _turnElapsed(object state)
        {
            Heading += (int)Turn * Config.SnakeTurnLength;
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
                case "TurnData":
                    Turn = (TurnState)JsonConvert.DeserializeObject<TurnRequest>(data).Turn;
                    break;
                case "Connect":
                    ConnectRequested?.Invoke(this, null);
                    break;
                case "Settings":
                    Send(JsonConvert.SerializeObject(new SettingsResponseModel() { Settings = Config.Constants }));
                    break;
            }
        }
        public enum SnakeState
        {
            Alive,
            Dead
        }
        public enum TurnState
        {
            Left = -1,
            Right = 1,
            None = 0
        }
    }
}
