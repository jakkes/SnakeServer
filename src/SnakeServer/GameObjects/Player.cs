﻿using Jakkes.WebSockets.Server;
using System.Collections.Generic;
using System;
using System.Threading;
using Newtonsoft.Json;
using SnakeServer.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SnakeServer.GameObjects
{
    public delegate void TurnStateChangedEventHandler(Player player, Player.TurnState state);
    public class Player
    {

        private Connection _conn;
        private List<Node> _nodes = new List<Node>();
        private Timer _moveTimer;
        private Timer _turnTimer;

        private int _length = Config.SnakeStartLength;

        public event EventHandler ConnectRequested;
        public event EventHandler Died;

        private GameDataModel _gameData;
        private Queue<ResponseModel> _modelsQueue = new Queue<ResponseModel>();


        public string ID { get { return _conn.ID; } }
        public double Heading { get; private set; }
        public int Length { get { return _length; } }
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
            _conn.MessageSent += _conn_MessageSent;
            _conn.Closed += _conn_Closed;
            _conn.Send(JsonConvert.SerializeObject(new ConnectResponseModel() { Id = ID }));
        }

        private void _conn_MessageSent(object sender, EventArgs e)
        {
            if (_modelsQueue.Count > 0)
            {
                var model = _modelsQueue.Dequeue();
                try { _send(JsonConvert.SerializeObject(model)); }
                catch (ConnectionBusyException) { _modelsQueue.Enqueue(model); }
            }
            //else if(_gameData != null)
            //{
            //    try
            //    {
            //        _send(JsonConvert.SerializeObject((_gameData)));
            //        _gameData = null;
            //    }
            //    catch (ConnectionBusyException) { }
            //}
        }
        public void Start(Node start, double heading)
        {
            State = SnakeState.Alive;
            Heading = heading;
            _length = Config.SnakeStartLength;
            _nodes = new List<Node>();
            _nodes.Add(start);
            _moveTimer = new Timer(new TimerCallback(Move), null, 0, 1000 / Config.SnakeMovementRate);

            Send(new GameStartResponseModel());
        }
        private void _send(string message)
        {
            try
            {
                _conn.Send(message);
            }
            catch (ConnectionClosedException)
            {
                Die();
            }
        }
        public void Send(ResponseModel data)
        {
            try
            { _send(JsonConvert.SerializeObject(data)); }
            catch (ConnectionBusyException)
            { _modelsQueue.Enqueue(data); }
        }
        public void Send(GameDataModel model)
        {
            try { _send(JsonConvert.SerializeObject(_gameData = model)); }
            catch (ConnectionBusyException) { _gameData = model; }
        }
        public void Die()
        {
            State = SnakeState.Dead;
            _moveTimer?.Dispose();
            _turnTimer?.Dispose();
            if (_conn.State == WebSocketState.Open)
            {
                Send(new DiedResponseModel());
            }
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
            for (int i = (int)(Config.SnakeRadius * 4 / Config.SnakeMovementLength); i < _nodes.Count; i++)
            {
                if (Head.DistanceTo(_nodes[i]) < 2 * Config.SnakeRadius)
                {
                    Die();
                    break;
                }
            }
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
                    try
                    {
                        Send(new SettingsResponseModel() { Settings = Config.Constants });
                    } catch (ConnectionBusyException) { _modelsQueue.Enqueue(new SettingsResponseModel() { Settings = Config.Constants }); }
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