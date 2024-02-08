using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Text;

using System.Net;
using System.Net.Sockets;

public class NetworkManager 
{
	public const int MaxTurns = 1000000;
	public const int FutureLength = 5;
	public const int MaxPacketsPerFrameCount = 10;
	public const ulong TimeBetweenHellos = 10;
	public const ulong StartDelay = 150;
	public const int SubTurnsPerTurn = 4;
	public const int MaxPlayerCount = 5;
	public const byte ChannelLimit = 0;

	// For data from connection and disconnection
	public const int Me = 0;

	// Invalid id
	public const int IdNotSet = 2000000000;

	// Events for network state machine
	public enum Event
	{
		Join, EnterStarting, EnterPlaying, NotEnoughToProcessTurn, DelayFinished,
		ConnectionRequested, ConnectionDone, RequestConnection, ConnectionRemain, IntroduceToPeers
	}

	// Creates ENet type packet
	public static ENet.Packet CreatePacket(string json)
	{
		ENet.Packet packet = new ENet.Packet();
		// convert string to stream
		byte[] data = Encoding.ASCII.GetBytes(json);

		packet.Initialize(data, 0, data.Length, ENet.PacketFlags.Reliable);
		return packet;
	}

	// For interface between string and stream
	public static string StreamToString(byte[] data)
	{
		MemoryStream stream = new MemoryStream(data);

		// convert stream to string
		StreamReader reader = new StreamReader(stream);
		string json = reader.ReadToEnd();
		return json;
	}

	// Actually it's not enough to connect with local ip
	public static string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		return "";
	}

	// Generates mater peer's state machine
	public static TinyStateMachine<NetworkState, Event> CreateMasterPeerFSM(NetworkManager mgr)
	{
		var lobby = new LobbyState(mgr);
		var starting = new StartingState(mgr);
		var playing = new PlayingState(mgr);
		var delay = new DelayState(mgr);
		var acceptConnect = new MasterAcceptConnectState(mgr);

		var fsm = new TinyStateMachine<NetworkState, Event>(lobby);
		fsm
			.Tr(lobby, Event.EnterStarting, starting)
			//.On(()=>Debug.Log("state change to \"StartingState\"\n"))

			.Tr(lobby, Event.ConnectionRequested, acceptConnect)
			//.On(() => Debug.Log("state change to \"MasterAcceptConnectState\"\n"))

			.Tr(acceptConnect, Event.ConnectionDone, lobby)
			//.On(() => Debug.Log("state change to \"LobbyState\"\n"))

			.Tr(starting, Event.EnterPlaying, playing)
			//.On(() => Debug.Log("state change to \"PlayingState\"\n"))

			.Tr(playing, Event.NotEnoughToProcessTurn, delay)
			//.On(() => Debug.Log("state change to \"DelayState\"\n"))

			.Tr(delay, Event.NotEnoughToProcessTurn, delay)
			//.On(() => Debug.Log("state change to \"DelayState\"\n"))

			.Tr(delay, Event.DelayFinished, playing)
			//.On(() => Debug.Log("state change to \"PlayingState\"\n"))
			;

		return fsm;
	}

	// Generates peer's state machine
	public static TinyStateMachine<NetworkState, Event> CreatePeerFSM(NetworkManager mgr)
	{
		var hello = new HelloState(mgr);
		var helloPeers = new HelloPeersState(mgr);
		var requestConnect = new RequestConnectState(mgr);
		var acceptConnect = new AcceptConnectState(mgr);
		var lobby = new LobbyState(mgr);
		var starting = new StartingState(mgr);
		var playing = new PlayingState(mgr);
		var delay = new DelayState(mgr);

		var fsm = new TinyStateMachine<NetworkState, Event>(hello);
		fsm
			.Tr(hello, Event.IntroduceToPeers, helloPeers)
			//.On(() => Debug.Log("state change to \"HelloPeersState\"\n"))

			.Tr(helloPeers, Event.ConnectionRemain, requestConnect)
			//.On(() => Debug.Log("state change to \"RequestConnectState\"\n"))

			.Tr(requestConnect, Event.ConnectionDone, helloPeers)
			//.On(() => Debug.Log("state change to \"HelloPeersState\"\n"))

			.Tr(helloPeers, Event.Join, lobby)
			//.On(() => Debug.Log("state change to \"LobbyState\"\n"))

			.Tr(lobby, Event.ConnectionRequested, acceptConnect)
			//.On(() => Debug.Log("state change to \"AcceptConnectState\"\n"))

			.Tr(acceptConnect, Event.ConnectionDone, lobby)
			//.On(() => Debug.Log("state change to \"LobbyState\"\n"))

			.Tr(lobby, Event.EnterStarting, starting)
			//.On(() => Debug.Log("state change to \"StartingState\"\n"))

			.Tr(starting, Event.EnterPlaying, playing)
			//.On(() => Debug.Log("state change to \"PlayingState\"\n"))

			.Tr(playing, Event.NotEnoughToProcessTurn, delay)
			//.On(() => Debug.Log("state change to \"DelayState\"\n"))

			.Tr(delay, Event.NotEnoughToProcessTurn, delay)
			//.On(() => Debug.Log("state change to \"DelayState\"\n"))

			.Tr(delay, Event.DelayFinished, playing)
			//.On(() => Debug.Log("state change to \"PlayingState\"\n"))
			;

		return fsm;
	}
		

	public class ReceivedPacket
	{
		public ENet.Event Event
		{
			get { return _event; }
		}

		public string Data
		{
			get { return _data; }
		}
		public ReceivedPacket(string data, ENet.Event @event)
		{
			_data = data;
			_event = @event;
		}

		private string _data;
		private ENet.Event _event;
	}


	public SortedDictionary <uint, CommandList> [] TurnData
	{
		get { return _turnData; }
	}

	public EngineLogic Engine
	{
		get { return _engine; }
	}

	public int TurnNumber
	{
		get { return _turnNumber; }
	}

	public int SubTurnNumber
	{
		get { return _subTurnNumber; }
		set { _subTurnNumber = value; }
	}


	public Queue<ReceivedPacket> PacketQ
	{
		get { return _packetQ; }
	}

	public Queue<ReceivedPacket> NotHandledPacketQ
	{
		get { return _notHandledPacketQ; }
	}

	public TinyStateMachine<NetworkState, Event> FSM
	{
		get { return _fsm; }
	}


	public int PlayerCnt
	{
		get { return _idToNameMap.Count; }
	}

	public SortedDictionary<uint, string> IdToNameMap
	{
		get { return _idToNameMap; }
	}

	public SortedDictionary<string, IpPort> NameToServerAddrMap
	{
		get { return _nameToServerAddrMap; }
	}

	public SortedDictionary<IpPort, uint> ServerAddrToIdMap
	{
		get { return _serverAddrToIdMap; }
	}

	public SortedDictionary<uint, ENet.Peer> IdToServerHostConnectedPeerMap
	{
		get { return _idToServerHostConnectedPeerMap; }
	}

	public SortedDictionary<uint, ENet.Peer> IdToClientHostConnectedPeerMap
	{
		get { return _idToClientHostConnectedPeerMap; }
	}

	public ENet.Host ServerHost
	{
		get { return _serverHost; }
	}

	public ENet.Host ClientHost
	{
		get { return _clientHost; }
	}

	public IpPort ServerHostIpPort
	{
		get { return _serverHostIpPort; }
	}



	public bool IsMasterPeer
	{
		get { return _isMasterPeer; }
	}

	public ENet.Peer MasterPeer
	{
		get { return _masterPeer; }
		set { _masterPeer = value; }
	}

	public bool ClientHostActivated
	{
		get { return _clientHostActivated; }
		set { _clientHostActivated = value; }
	}



	public string Name
	{
		get { return _name; }
	}

	public uint PlayerId
	{
		get { return _playerId; }
		set { _playerId = value; }
	}

	public uint HighestPlayerId
	{
		get { return _highestPlayerId; }
		set { _highestPlayerId = value; }
	}


	public ulong Time
	{
		get { return _time; }
		set { _time = value; }
	}

	public ulong TimeOfLastHello
	{
		get { return _timeOfLastHello; }
		set { _timeOfLastHello = value; }
	}

	public ulong TimeToStart
	{
		get { return _timeToStart; }
		set { _timeToStart = value; }
	}
		

	public Queue<IpPort> ConnectionBuffer
	{
		get { return _connectionBuffer; }
	}

	// Init as master peer.
	public NetworkManager(EngineLogic engine, int port, string name)
	{
		Debug.Log("Initializing master peer...");

		_engine = engine;

		_turnData = new SortedDictionary<uint, CommandList> [MaxTurns];
		for(int i=0; i < MaxTurns; i++)
			_turnData [i] = new SortedDictionary<uint, CommandList> ();

		_turnNumber = -FutureLength;
		_subTurnNumber = 0;
		_router = new ENetUDPRouter (this);

		_packetQ = new Queue<ReceivedPacket>();
		_notHandledPacketQ = new Queue<ReceivedPacket> ();
		_fsm = CreateMasterPeerFSM(this);

		_idToNameMap = new SortedDictionary<uint, string>();
		_serverAddrToIdMap = new SortedDictionary<IpPort, uint>(new IpPort.Comparer());
		_nameToServerAddrMap = new SortedDictionary<string, IpPort>();
		_idToServerHostConnectedPeerMap = new SortedDictionary<uint, ENet.Peer>();
		_idToClientHostConnectedPeerMap = new SortedDictionary<uint, ENet.Peer>();

		_serverHost = new ENet.Host();
		_clientHost = new ENet.Host();
		_serverHost.InitializeServer(port, MaxPlayerCount);
		_clientHost.Initialize(null, MaxPlayerCount);
		_serverHostIpPort = new IpPort(GetLocalIPAddress(), port);

		_isMasterPeer = true;
		_clientHostActivated = false;

		_name = name;
		_playerId = 1;
		_highestPlayerId = _playerId;
		_idToNameMap.Add(_playerId, _name);

		_time = 0;
		_timeOfLastHello = 0;
		_timeToStart = 0;

		_connectionBuffer = new Queue<IpPort>();
	}


	// Init as peer
	public NetworkManager(EngineLogic engine, string masterIp, int masterPort, int myPort, string name)
	{
		Debug.Log("Initializing peer...");

		_engine = engine;
		_engine.SetConnectingState ();
		_turnData = new SortedDictionary<uint, CommandList> [MaxTurns];

		for(int i = 0; i < MaxTurns; i++)
			_turnData [i] = new SortedDictionary<uint, CommandList> ();

		_turnNumber = -FutureLength;
		_subTurnNumber = 0;
		_router = new ENetUDPRouter (this);

		_packetQ = new Queue<ReceivedPacket>();
		_notHandledPacketQ = new Queue<ReceivedPacket> ();
		_fsm = CreatePeerFSM(this);

		_serverAddrToIdMap = new SortedDictionary<IpPort, uint>(new IpPort.Comparer());
		_idToNameMap = new SortedDictionary<uint, string>();
		_nameToServerAddrMap = new SortedDictionary<string, IpPort>();
		_idToServerHostConnectedPeerMap = new SortedDictionary<uint, ENet.Peer>();
		_idToClientHostConnectedPeerMap = new SortedDictionary<uint, ENet.Peer>();

		_serverHost = new ENet.Host();
		_clientHost = new ENet.Host();
		_serverHost.InitializeServer(myPort, MaxPlayerCount);
		_clientHost.Initialize(null, MaxPlayerCount);
		_serverHostIpPort = new IpPort(GetLocalIPAddress(), myPort);

		_isMasterPeer = false;
		_masterPeer = _clientHost.Connect(masterIp, masterPort, IdNotSet);
		_clientHostActivated = true;

		_name = name;
		_playerId = IdNotSet;

		_time = 0;
		_timeOfLastHello = 0;
		_timeToStart = 0;

		_connectionBuffer = new Queue<IpPort>();
	}




	public void ProcessIncomings()
	{
		_time++;
		// Read and push into Q(if clients are divided)
		ReadIncomingPacketsToQueue();

		ProcessQueuedPackets ();

		//UpdateBytesSentLastFrame();
	}

	public void ReadIncomingPacketsToQueue()
	{
		_router.Recv ();
	}


	void ProcessQueuedPackets()
	{
		while (_packetQ.Count > 0)
		{
			ReceivedPacket nextPacket = _packetQ.Dequeue();

			// <Additional> Look for latency with timing.

			_fsm.State.HandlePackets(nextPacket.Data, nextPacket.Event);

		}
	}

	public void HandleTurn(IpPort ipPort, int turnNumer, uint playerId, string cmdJson)
	{
		if (ServerAddrToIdMap.ContainsKey(ipPort))
		{
			uint expectedId = ServerAddrToIdMap[ipPort];

			if ( playerId != expectedId )
			{
				Debug.Log( "We received turn data for a different player Id...stop trying to cheat!" );
				return;
			}

			if(!TurnData[ turnNumer ].ContainsKey(playerId))
				TurnData[ turnNumer ].Add(playerId, CommandList.Create(cmdJson));

			else
				Debug.Log ("<" + turnNumer + " " + playerId + ">   already exist key!");
		}
	}


	public void ProcessOutgoings()
	{
		_fsm.State.UpdateAndSend();
	}


	public void EnterPlayingState()
	{
		_fsm.Fire(Event.EnterPlaying);

		// World, InputManager and others ..
		_engine.SetPlayingState ();
	}
		
	public void SendTurnPackets()
	{
		uint playerId = _engine.NetworkMgr.PlayerId;
		string cmdsJson = _engine.InputMgr.SavedCommands.ToStream();

		TurnPacket packet = new TurnPacket (_turnNumber + FutureLength, playerId, cmdsJson);
		SendPacket(JsonMapper.ToJson (packet));

        // The ordering could change after json parsing, so make it process same to others.
        CommandList cmdlist = CommandList.Create(cmdsJson);
        
		// Save our turn data for TurnNumber + FutureLength
		if(!_turnData[ _turnNumber + FutureLength ].ContainsKey(playerId))
			_turnData[ _turnNumber + FutureLength ].Add(playerId, cmdlist);
		else
			Debug.Log ("<" + (_turnNumber + FutureLength) + " " + playerId + ">   already exist key!");

        _engine.InputMgr.SavedCommands.Clear ();
	}

	void SendPacket(string json)
	{
		_router.Send (json);
	}

	public void TryTurn()
	{
		// A negative turn means there's no possible commands yet
		if (_turnNumber < 0) 
		{
			_subTurnNumber = 0;
			_turnNumber++;
			return;
		}

		// Only advance the turn IF we received the data for everyone
		if ( _turnData[ _turnNumber + 1 ].Count == PlayerCnt )
		{
			if ( _fsm.State.GetType() == typeof(DelayState) )
			{
				// Throw away any input accured during delay
				_engine.InputMgr.SavedCommands.Clear();
				_fsm.Fire (Event.DelayFinished);
			}
            
            // Process all the moves for this turn
            foreach ( var e in _turnData[ _turnNumber ] )
			{
                if (e.Value != null)
                {
                    e.Value.ProcessCommands(_engine);
                    if (e.Value.Count() > 0)
                    {
                        Debug.Log("turn : " + _turnNumber + " cmd : " + e.Value.Count() + " wstep : " + _engine.World.Step);
                    }
                    else
                    {
                        Debug.Log("t:" + _turnNumber);
                    }
                }
			}
            _turnNumber++;
            _subTurnNumber = 0;

        }
		else
		{
			// Don't have all player's turn data, we have to delay :(
			_fsm.Fire(Event.NotEnoughToProcessTurn);
		}
	}


	public void TryStartGame()
	{
		if (_isMasterPeer && _fsm.State.GetType() == typeof(LobbyState))
		{
			Debug.Log("Master peer starting the game!");

			// Make seed
			uint seed = (uint)UnityEngine.Time.realtimeSinceStartup;

			StartPacket start = new StartPacket(seed);
			var enetPacket = CreatePacket(JsonMapper.ToJson(start));

			_serverHost.Broadcast(ChannelLimit, ref enetPacket);

			_timeToStart = StartDelay;
			_fsm.Fire(Event.EnterStarting);
			_engine.SetStartingState (seed);
		}
	}

	public void UpdateHighestPlayerId(uint inId)
	{
		_highestPlayerId = IMath.Max(_highestPlayerId, inId);
	}


	public void HandleConnectionReset( uint playerId )
	{
		// This means it already erased.
		if (!IdToNameMap.ContainsKey(playerId))
			return;

		string name = IdToNameMap[playerId];
		IpPort ipPort = NameToServerAddrMap[name];
		IdToServerHostConnectedPeerMap[playerId].Disconnect(0);
		IdToClientHostConnectedPeerMap[playerId].Disconnect(0);
		IdToServerHostConnectedPeerMap.Remove(playerId);
		IdToClientHostConnectedPeerMap.Remove(playerId);
		IdToNameMap.Remove(playerId);
		ServerAddrToIdMap.Remove(ipPort);
		NameToServerAddrMap.Remove(name);

		// For new peers who doesn't know the peer gone out while connecting
		if (IsMasterPeer)
		{
			DisConnectedPacket disConnect = new DisConnectedPacket(playerId);
			var enetPacket = CreatePacket(JsonMapper.ToJson(disConnect));
			ServerHost.Broadcast(ChannelLimit, ref enetPacket);
		}
		else
		{
			// If this was the master peer, pick the next player in the string map to be MP
			if (ipPort.EndPoint().Equals(MasterPeer.GetRemoteAddress()))
			{
				uint newMPId = IdNotSet;
				foreach (var i in IdToNameMap)
				{
					if (newMPId > i.Key)
						newMPId = i.Key;
				}
				// I'm the new master peer, muahahahah
				if (newMPId == PlayerId)
					SetAsNewMasterPeer();
				else
					MasterPeer = IdToClientHostConnectedPeerMap[newMPId];
			}
		}
		// If we were in delay, then let's see if we can continue now that this player DC'd?
		if (_fsm.State.GetType() == typeof(DelayState))
        {
            TryTurn();
        }
		Briefing();
	}

	public void Remove()
	{
		foreach (var i in _idToClientHostConnectedPeerMap)
			if (i.Value.IsInitialized) {
				i.Value.DisconnectLater (0);
			}

		foreach (var i in _idToServerHostConnectedPeerMap)
			if (i.Value.IsInitialized) {
				i.Value.DisconnectLater (0);
			}

		if (!_isMasterPeer && _masterPeer.IsInitialized) {
			_masterPeer.DisconnectLater (0);
		}
		if (_serverHost.IsInitialized) {
			_serverHost.Dispose ();
		}
		if (_clientHost.IsInitialized) {
			if(_clientHostActivated)
				_clientHost.Dispose ();
		}
	}

	public void SetAsNewMasterPeer()
	{
		_isMasterPeer = true;
		_fsm = CreateMasterPeerFSM(this);
	}


	public void Briefing()
	{
		Debug.Log (" ");
		Debug.Log ("+-----------------------------------------------------------+");
		Debug.Log ("  server host address: " + _serverHostIpPort);
		Debug.Log ("  isMasterPeer: " + _isMasterPeer);
		Debug.Log ("  name: " + _name);
		Debug.Log ("  playerId: " + _playerId);
		Debug.Log ("  highestPlayerId: " + _highestPlayerId);
		Debug.Log ("  FSM state: " + _fsm.State);
		Debug.Log ("  Not Handled Packets: " + _notHandledPacketQ.Count);
		//Debug.Log("  Event buffer count: " + _eventBuffer.Count);

		Debug.Log (" ");
		Debug.Log ("  [Id] [Peer] Map");
		Debug.Log ("  count: " + _idToServerHostConnectedPeerMap.Count);
		foreach(var e in _idToServerHostConnectedPeerMap)
		{
			Debug.Log ("  " + "(" + e.Key + ", " + e.Value.GetRemoteAddress () + ")");
		}

		Debug.Log (" ");
		Debug.Log ("  [Id] [ClientPeer] Map");
		Debug.Log ("  count: " + _idToClientHostConnectedPeerMap.Count);
		foreach (var e in _idToClientHostConnectedPeerMap)
		{
			Debug.Log ("  " + "(" + e.Key + ", " + e.Value.GetRemoteAddress () + ")");
		}

		Debug.Log (" ");
		Debug.Log ("  [Address] [Id] Map");
		Debug.Log ("  count: " + _serverAddrToIdMap.Count);
		foreach (var e in _serverAddrToIdMap)
		{
			Debug.Log ("  " + "(" + e.Key.Addr + ", " + e.Value + ")");
		}

		Debug.Log (" ");
		Debug.Log ("  [Name] [Address] Map");
		Debug.Log ("  count: " + _nameToServerAddrMap.Count);
		foreach (var e in _nameToServerAddrMap)
		{
			Debug.Log ("  " + "(" + e.Key + ", " + e.Value + ")");
		}

		Debug.Log (" ");
		Debug.Log ("  [Id] [Name] Map");
		Debug.Log ("  count: " + _idToNameMap.Count);
		foreach (var e in _idToNameMap)
		{
			Debug.Log ("  " + "(" + e.Key + ", " + e.Value + ")");
		}

		Debug.Log ("+-----------------------------------------------------------+");
	}


	private EngineLogic _engine;

	// This stores all of our turn information for every turn since game start.
	// Index of array is turn number, key is playerID and value is turn data.
	private SortedDictionary<uint, CommandList> [] _turnData;
	private int _turnNumber;
	private int _subTurnNumber;

	private ENetUDPRouter _router;

	private Queue<ReceivedPacket> _packetQ;
	private Queue<ReceivedPacket> _notHandledPacketQ;
	private TinyStateMachine<NetworkState, Event> _fsm;

	private SortedDictionary<uint, string> _idToNameMap;
	private SortedDictionary<string, IpPort> _nameToServerAddrMap;
	private SortedDictionary<IpPort, uint> _serverAddrToIdMap;
	private SortedDictionary<uint, ENet.Peer> _idToServerHostConnectedPeerMap;
	private SortedDictionary<uint, ENet.Peer> _idToClientHostConnectedPeerMap;

	private ENet.Host _serverHost;
	private ENet.Host _clientHost;
	private IpPort _serverHostIpPort;

	private bool _isMasterPeer;
	private ENet.Peer _masterPeer;
	private bool _clientHostActivated;

	private string _name;
	private uint _playerId;
	private uint _highestPlayerId;

	private ulong _time;
	private ulong _timeOfLastHello;
	private ulong _timeToStart;

	Queue<IpPort> _connectionBuffer;
}
