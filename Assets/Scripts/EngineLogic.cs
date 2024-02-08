using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Net.Sockets;


public class EngineLogic : MonoBehaviour 
{
    public static Color[] Colors;

    public World World
	{
		get { return _gameWorld; }
	}

	public EntityManager EntityMgr
	{
		get { return _entityMgr; }
	}

	public NetworkManager NetworkMgr
	{
		get { return _networkMgr; }
	}

	public InputManager InputMgr
	{
		get { return _inputMgr; }
	}

	public UIManager UIMgr
	{
		get { return _uiMgr; }
	}

	public Database Database
	{
		get { return _database; }
	}

    public uint RandGen(int min, int max)
	{
		return _random.Next(min, max);
	}

	public uint RandGen(uint max)
	{
        return _random.Next(max);
	}

	public void TrainButtonClicked(int buttonIndex)
	{
		IClickable current = _inputMgr.ClickDownEntity;
		if (current == null) 
			return;

		if (!(current is Building))
			return;

		Building building = current as Building;
        
		string name = building.UnitDeck [buttonIndex];
		Database.CharacterData data = _database.CharacterDataEntry (name);

        // If building ..
        if (data == null)
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            list.Add(new KeyValuePair<int, string>((int)World.Relation.CreateByBuild, name));

            _inputMgr.SavedCommands.AddCommand(
                new TrainCharacterCommand(
                    _networkMgr.PlayerId,
                    building.Id,
                    "Builder",
                    list));
        }
        else
        {
            if (_uiMgr.Gold - data.NeedGold < 0)
                return;
            
            _inputMgr.SavedCommands.AddCommand(
                new TrainCharacterCommand(
                    _networkMgr.PlayerId,
                    building.Id,
                    name,
                    new List<KeyValuePair<int, string>>()));
        }
	}

	public void HolePunching(string globalIp, string localIp, int globalPort, int localPort)
	{
		ENet.Host globalServerHost = new ENet.Host ();
		ENet.Host localServerHost = new ENet.Host ();

		globalServerHost.InitializeServer(globalPort, 10);
		localServerHost.InitializeServer(localPort, 10);

		ENet.Host globalClientHost = new ENet.Host ();
		ENet.Host localClientHost = new ENet.Host ();

		globalClientHost.Initialize(null, 10);
		localClientHost.Initialize(null, 10);

		ENet.Peer globalPeer = globalClientHost.Connect(globalIp, globalPort, 0);
		ENet.Peer localPeer = localClientHost.Connect(localIp, localPort, 0);

		while (true) 
		{
			if (Service (globalServerHost))
				return;
			if (Service (localServerHost))
				return;
			if (Service (globalClientHost))
				return;
			if (Service (localClientHost))
				return;
		}
	}

	public bool Service(ENet.Host host)
	{
		ENet.Event @event;
		if (host.Service(10, out @event))
		{
			do
			{
				switch (@event.Type)
				{
				case ENet.EventType.Connect:
					Debug.Log("Connect! in HolePunching");
					return true;

				case ENet.EventType.Receive:
					Debug.Log("Receive! in HolePunching");
					return true;

				case ENet.EventType.Disconnect:
					Debug.Log("Disconnect! in HolePunching");
					return true;
				}
			}
			while (host.CheckEvents(out @event));
		}
		return false;
	}


	public void LoginButtonClicked()
	{
		_uiMgr.LoginCanvas.enabled = false;

		string name = _uiMgr.LoginNameText.text;

		int masterPort = -1;
		if (_uiMgr.LoginMasterPortText.text != "")
			masterPort = int.Parse (_uiMgr.LoginMasterPortText.text);

		string masterIp = _uiMgr.LoginMasterIpText.text;

		if(masterIp == "")
			masterIp = "127.0.0.1";

		int port = -1;
		if(_uiMgr.LoginPortText.text != "")
			port = int.Parse(_uiMgr.LoginPortText.text);

		Debug.Log (name);
		Debug.Log (masterPort);
		Debug.Log (masterIp);
		Debug.Log (port);


		string globalIp = "";
		string localIp = "";
		int globalPort = 0;
		int localPort = globalPort + 2;


		//HolePunching (globalIp, localIp, globalPort, localPort);
        

		if(_uiMgr.LoginPortText.text == "")
			_networkMgr = new NetworkManager (this, masterPort, name);
		else
			_networkMgr = new NetworkManager (this, masterIp, masterPort, port, name);

		SetLobbyState ();
	}


	public void StartButtonClicked()
	{
		_networkMgr.TryStartGame ();
	}

	public void ExitButtonClicked()
	{
		#if UNITY_EDITOR
		if(_networkMgr != null)
			_networkMgr.Remove();
		Application.Quit();
		#else
		if(_networkMgr != null)
			_networkMgr.Remove();
		Application.Quit();
		#endif 
	}

    public void MinimapTouched()
    {
        if(_inputMgr != null)
            _inputMgr.ClickMinimap();
    }

    public enum CommandButton
    {
        None, Attack, Move
    }

    public void AttackButtonClicked()
    {
        if (_inputMgr != null)
        {
            _inputMgr.HoldingBox.CurrentPressedButton = (int)CommandButton.Attack;
            _inputMgr.HoldingBox.Fsm.Fire(HoldingBox.Event.ButtonPressed);
        }
    }

    public void MoveButtonClicked()
    {
        if (_inputMgr != null)
        {
            _inputMgr.HoldingBox.CurrentPressedButton = (int)CommandButton.Move;
            _inputMgr.HoldingBox.Fsm.Fire(HoldingBox.Event.ButtonPressed);
        }
    }


    public IEnumerator Wait (float time)
	{
		yield return new WaitForSeconds (time);
	}

	public void SetLobbyState()
	{
		Debug.Log ("Engine setting to LobbyState ..");
		_uiMgr.ConnectingCanvas.enabled = false;
		_uiMgr.LobbyCanvas.enabled = true;
        _uiMgr.LobbyText.text =
            "Ip : " + _networkMgr.ServerHostIpPort.Ip +
            "       Port : " + _networkMgr.ServerHostIpPort.Port +
            "\nName : " + _networkMgr.Name + "\n";

        foreach (var e in _networkMgr.IdToNameMap)
        {
            _uiMgr.LobbyText.text = _uiMgr.LobbyText.text + "\nId : " + e.Key + "       Name : " + e.Value; 
        }
	}

	public void SetConnectingState()
	{
		Debug.Log ("Engine setting to WelcomeState ..");
		_uiMgr.ConnectingCanvas.enabled = true;
		_uiMgr.LobbyCanvas.enabled = false;
	}

	public void SetStartingState(uint seed)
	{
		Debug.Log ("Engine setting to StartingState ..  random seed : " + seed);
		_uiMgr.LobbyCanvas.enabled = false;
		_uiMgr.ConnectingCanvas.enabled = true;

		_random = new WRandom (seed);
	}

	const int AlignSize = 200;
	const int CornerSpaceSize = 1000;
	enum PlaceHolders : int
	{
		SoulStoneMiddle, SoulStoneWing1, SoulStoneWing2,
		BuildingMiddle, BuildingWing1, BuildingWing2,
		TowerMiddle, TowerWing1, TowerWing2,
		PlaceHolderSize
	};

	enum Corners : int
	{
		LeftDown,  RightDown, RightUp, LeftUp,
		CornerSize
	};

    public void TestEntities(string name, int num, uint player, Point2D pos)
    {
        for (int i = 0; i < num; i++)
        {
            Point2D noise = new Point2D((int)RandGen(-5, 5), (int)RandGen(-5, 5));
            InputMgr.SavedCommands.AddCommand(
                new GenCharacterCommand(player, pos + noise, name, new List<KeyValuePair<int, string>>()));
        }
    }

    public void SetPlayingState()
	{
		Debug.Log ("Engine setting to PlayingState ..");
		_uiMgr.ConnectingCanvas.enabled = false;
		_uiMgr.PlayingCanvas.enabled = true;
        _uiMgr.MinimapCamera.enabled = true;
        _uiMgr.MinimapCanvas.enabled = true;

        _inputMgr = new InputManager(this);
        
        Vector3 leftBottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 rightTop = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        
        int minX = -2000;
		int minY = -2000;
		int maxX = 2000;
		int maxY = 2000;

		uint playerId = _networkMgr.PlayerId - 1;
        
		// 4 starting points of each corners
		Point2D[] corners = new Point2D[(int)Corners.CornerSize] 
		{
			new Point2D (minX + CornerSpaceSize, minY + CornerSpaceSize),	//1
			new Point2D (maxX - CornerSpaceSize, maxY - CornerSpaceSize),	//3
			new Point2D (maxX - CornerSpaceSize, minY + CornerSpaceSize),	//2
			new Point2D (minX + CornerSpaceSize, maxY - CornerSpaceSize)	//4
		};
        
        Point2D[] headings = new Point2D[(int)Corners.CornerSize] 
		{
			new Point2D (0, 1),		//1
			new Point2D (0, -1),	//3
			new Point2D (-1, 0),	//2
			new Point2D (1, 0)		//4
		};

		Point2D[] sides = new Point2D[(int)Corners.CornerSize] 
		{
			new Point2D (-1, 0),	//1
			new Point2D (1, 0),		//3
			new Point2D (0, -1),	//2
			new Point2D (0, 1)		//4
		};

        

		// 9 place holders for each type of buildings
		Point2D[] places = new Point2D[(int)PlaceHolders.PlaceHolderSize] 
		{
            new Point2D(-AlignSize, AlignSize),
            new Point2D(0, AlignSize * 2),
            new Point2D(-AlignSize * 2, 0),

            new Point2D(0, 0), 
			new Point2D(-AlignSize, -AlignSize), 
			new Point2D(AlignSize, AlignSize),

			new Point2D(AlignSize, -AlignSize), 
			new Point2D(0, -AlignSize * 2), 
			new Point2D(AlignSize * 2, 0)
		};

        Camera minicam = _uiMgr.MinimapCamera;
        int mheight = 2550;
        int mwidth = 3500;

        Debug.Log("world width height : " + mwidth + " " + mheight);
        
        _gameWorld = new World (this, -mwidth, -mheight, mwidth, mheight, new Point2D(0, 0), headings[playerId], sides[playerId]);

        

        int[] pIdToIndex = new int[] { 0, 2, 1, 3 };
        int index = pIdToIndex[playerId];

        Debug.Log(playerId + " " + index + " " + corners[index].x + " " + corners[index].y);

        Camera.main.transform.position = new Vector3(corners[playerId].x, corners[playerId].y, -20);
        _inputMgr.MinimapBox.MouseDown(this, corners[playerId]);

        //TestEntities("Cow", 75, 1, new Point2D(-150,-150));
        //TestEntities("Cow", 75, 2, new Point2D(200, 200));

        //Point2D[] heading = new Point2D[2] { new Point2D(0, -1), new Point2D(0, 1) };
        //Point2D[] side = new Point2D[2] { new Point2D(1, 0), new Point2D(-1, 0) };
        //Point2D[] heading = new Point2D[2] { new Point2D(1, 0), new Point2D(-1, 0) };
        //Point2D[] side = new Point2D[2] { new Point2D(0, 1), new Point2D(0, -1) };


        if (NetworkMgr.IsMasterPeer == true) 
		{
			for (int i = 0; i < IMath.Max((int)NetworkMgr.PlayerCnt, 2); i++) 
			{
                uint pId = (uint)(i + 1);
                for (int j = 0; j < (int)PlaceHolders.PlaceHolderSize; j++) 
				{
					// Rotate
					Point2D rotatePlace = C2DMatrix.Vec2DRotate(places [j], headings [i], sides [i]);
					Point2D pholder = corners [i] + rotatePlace;
                    
					string name = "";

                    if (j < 3)
                        name = "SoulStone";
                    else if (j < 6)
						name = "StatusBoss";
					else
						name = "Izual";

                    //Point2D pos =  C2DMatrix.PointToLocalSpace(pholder, headings[playerId], sides[playerId], new Point2D(0, 0));
                    InputMgr.SavedCommands.AddCommand (new CreateBuildingCommand (pId, name, pholder));
                }
			}
        }
	}

	public string LocalIPAddress()
	{
		IPHostEntry host;
		List<string> foundLocals = new List<string>();

		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			//Debug.Log (ip.ToString ());
			if (ip.AddressFamily == AddressFamily.InterNetwork) 
			{
				//foundLocals.Add (ip.ToString ());
				return ip.ToString ();
			}
		}

		return "";
	}

	// Use this for initialization
	void Start () 
	{
		CommandFactory.StaticInit ();
        
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = true;
		Screen.orientation = ScreenOrientation.AutoRotation;

		//Screen.orientation = ScreenOrientation.LandscapeLeft;
        
		LocalIPAddress ();

        Colors = new Color[4]
        {
            new Color(1F, 0.75F, 0.75F, 1F),
            new Color(0.75F, 1F, 0.75F, 1F),
            new Color(0.75F, 0.75F, 1F, 1F),
            new Color(0.75F, 0.75F, 0.75F, 1F)
        };

        // Manager setting..
        // _entityMgr must be initialized before world
        _entityMgr = new EntityManager (this);
		_database = new Database (this);
		_uiMgr = new UIManager(this);
		_networkMgr = null;
		_inputMgr = null;
		_gameWorld = null;
		_random = null;
	}

	void Update()
	{
		if(_inputMgr != null)
			_inputMgr.Update ();

        if (_gameWorld != null)
          _gameWorld.CellSpace.Render();
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if (_networkMgr != null) 
		{
            float startTime = Time.realtimeSinceStartup;
            _networkMgr.ProcessIncomings ();
			_networkMgr.ProcessOutgoings ();
            float curTime = Time.realtimeSinceStartup - startTime;
        }
	}

	private World _gameWorld;
	private EntityManager _entityMgr;
	private NetworkManager _networkMgr;
	private InputManager _inputMgr;
	private UIManager _uiMgr;
	private WRandom _random;

	private Database _database;

	private LanManager _lanManager;
}
