using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager 
{
	public Canvas TrainCanvas
	{
		get { return _trainCanvas; }
	}

	public Text GoldText
	{
		get { return _goldText; }
	}

    public Text GoldLabel
    {
        get { return _goldLabel; }
    }

    public Canvas LoginCanvas
	{
		get { return _loginCanvas; }
	}

	public Text LoginNameText
	{
		get { return _loginName; }
	}

	public Text LoginMasterPortText
	{
		get { return _loginMasterPort; }
	}

	public Text LoginMasterIpText
	{
		get { return _loginMasterIp; }
	}

	public Text LoginPortText
	{
		get { return _loginPort; }
	}

	public Canvas PlayingCanvas
    {
		get { return _playingCanvas; }
	}

	public Canvas ConnectingCanvas
	{
		get { return _connectingCanvas; }
	}

	public Canvas LobbyCanvas
	{
		get { return _lobbyCanvas; }
	}

	public Text LobbyText
	{
		get { return _lobbyText; }
	}

	public int Gold
	{
		get { return _gold; }
	}

    public Camera MinimapCamera
    {
        get { return _minimapCamera; }
    }

    public Canvas MinimapCanvas
    {
        get { return _minimapCanvas; }
    }

    public Button MinimapTouch
    {
        get { return _minimapTouch; }
    }

    public Button AttackButton
    {
        get { return _attackButton; }
    }

    public Button MoveButton
    {
        get { return _moveButton; }
    }

    public Text [] TrainCountText
    {
        get { return _trainCountText; }
    }


    public void SetGold(int gold)
	{
		_gold = gold;
		_goldText.text = gold.ToString ();
	}

	public UIManager(EngineLogic engine)
	{
		_engine = engine;
        
		_baseCanvas = GameObject.FindWithTag ("BaseCanvas").GetComponent<Canvas> ();
		_loginCanvas = GameObject.FindWithTag ("LoginCanvas").GetComponent<Canvas> ();
		_loginName = GameObject.FindWithTag ("LoginName").GetComponent<Text> ();
		_loginMasterPort = GameObject.FindWithTag ("LoginMasterPort").GetComponent<Text> ();
		_loginMasterIp = GameObject.FindWithTag ("LoginMasterIp").GetComponent<Text> ();
		_loginPort = GameObject.FindWithTag ("LoginPort").GetComponent<Text> ();
		_playingCanvas = GameObject.FindWithTag ("PlayingCanvas").GetComponent<Canvas> ();
        _playingCanvas.enabled = false;
        _gold = 500;
		_goldText = GameObject.FindWithTag("GoldText").GetComponent<Text>();
		_goldText.text = _gold.ToString();
        _goldLabel = GameObject.FindWithTag("GoldLabel").GetComponent<Text>();
        _attackButton = GameObject.FindWithTag("AttackButton").GetComponent<Button>();
        _moveButton = GameObject.FindWithTag("MoveButton").GetComponent<Button>();


        _trainCanvas = GameObject.FindWithTag("TrainCanvas").GetComponent<Canvas>();
		_trainCanvas.enabled = false;
		_trainButtons = new Button[3];
		for(int i=0; i<_trainButtons.Length; i++)
		{
			_trainButtons[i] = GameObject.FindWithTag ("TrainButton" + i).GetComponent<Button>();
		}

        _trainCountText = new Text[3];
        for (int i = 0; i < _trainCountText.Length; i++)
        {
            _trainCountText[i] = 
                GameObject.FindWithTag("TrainCountText" + i).GetComponent<Text>();
        }

        _trainRect = GameObject.FindWithTag ("TrainViewPort").GetComponent<ScrollRect>();

		_infoCanvas = GameObject.FindWithTag ("InfoCanvas").GetComponent<Canvas> ();
		_infoImage = GameObject.FindWithTag("InfoImage").GetComponent<Button>();
		_infoName = GameObject.FindWithTag("InfoName").GetComponent<Text>();
		_infoHp = GameObject.FindWithTag("InfoHp").GetComponent<Text>();
		_infoDamage = GameObject.FindWithTag("InfoDamage").GetComponent<Text>();
		_infoCanvas.enabled = false;

		_connectingCanvas = GameObject.FindWithTag ("ConnectingCanvas").GetComponent<Canvas> ();
		_connectingCanvas.enabled = false;

		_lobbyCanvas = GameObject.FindWithTag ("LobbyCanvas").GetComponent<Canvas> ();
		_lobbyCanvas.enabled = false;
		_lobbyText = GameObject.FindWithTag ("LobbyText").GetComponent<Text> ();

        _minimapCamera = GameObject.FindWithTag("MinimapCamera").GetComponent<Camera>();
        _minimapCamera.enabled = false;
        
        _minimapCanvas = GameObject.FindWithTag("MinimapCanvas").GetComponent<Canvas>();
        _minimapCanvas.enabled = false;
        _minimapTouch = GameObject.FindWithTag("MinimapTouch").GetComponent<Button>();
    }

	public bool SwallowEventOnUI(GameObject selected)
	{
		if (selected == null)
			return true;

		foreach(var ui in _trainButtons)
		{
			if (selected.tag == ui.tag)
				return false;
		}

        if (_minimapTouch.tag == selected.tag)
            return false;

        if (_attackButton.tag == selected.tag)
            return false;

        if (_moveButton.tag == selected.tag)
            return false;

        return true;
	}


	public void ShowTrainUnitList(bool show, string [] unitDeck = null)
	{
		if (show) 
		{
			_trainCanvas.enabled = true;
			for(int i = 0; i < _trainButtons.Length; i++)
			{
				string name = unitDeck [i];
				var data = _engine.Database.CharacterDataEntry (name);

                if (data == null)
                {
                    var bdata = _engine.Database.BuildingDataEntry(name);
                    _trainButtons[i].GetComponent<Image>().sprite = AssetManagerLogic.instance.InfoSpriteMap[name];
                    _trainButtons[i].GetComponentInChildren<Text>().text = name + " (" + bdata.NeedGold + ")";
                }
                else
                {
                    _trainButtons[i].GetComponent<Image>().sprite = AssetManagerLogic.instance.SpriteMap[name]["Walk"][0];
                    _trainButtons[i].GetComponentInChildren<Text>().text = name + " (" + data.NeedGold + ")";
                }
			}
		}
		else 
		{
			_trainCanvas.enabled = false;
		}
	}

	public void ShowInfomation(bool show, string entityName = "", int hp = 0, int damage = 0)
	{
		if (show) 
		{
			_infoCanvas.enabled = true;
			_infoImage.image.sprite = AssetManagerLogic.instance.InfoSpriteMap [entityName];

			_infoName.text = entityName;
			_infoHp.text = hp.ToString();
			_infoDamage.text = damage.ToString();
		}
		else 
		{
			_infoCanvas.enabled = false;
		}
	}

	EngineLogic _engine;

	Canvas _baseCanvas;

	// Login
	Canvas _loginCanvas;
	Text _loginName;
	Text _loginMasterPort;
	Text _loginMasterIp;
	Text _loginPort;

	// Lobby
	Canvas _lobbyCanvas;
	Text _lobbyText;

	// For training list when building is clicked
	Canvas _trainCanvas;
	ScrollRect _trainRect;
	Button [] _trainButtons;
    Text[] _trainCountText;
    
    // For score board
    Canvas _playingCanvas;
	Text _goldText;
    Text _goldLabel;

	int _gold;

	// For infomation when entity is clicked
	Canvas _infoCanvas;
	Button _infoImage;
	Text _infoName;
	Text _infoHp;
	Text _infoDamage;

	Canvas _connectingCanvas;

    Camera _minimapCamera;
    Canvas _minimapCanvas;
    Button _minimapTouch;

    Button _attackButton;
    Button _moveButton;
}
