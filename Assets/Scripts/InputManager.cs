using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System;
using UnityEngine.EventSystems;


public class TargetLine
{
	public Point2D ClickDownPos
	{
		get { return _clickDownPos; }
	}

	public Point2D ClickUpPos
	{
		get { return _clickUpPos; }
	}

    public Color Color
    {
        set
        {
            _targetLine.startColor = value;
            _targetLine.endColor = value;
            _selectedTarget.GetComponent<SpriteRenderer>().color = value;
            _releasedTarget.GetComponent<SpriteRenderer>().color = value;
        }
    }

    public bool Enable
    {
        set
        {
            _targetLine.enabled = value;
            _selectedTarget.GetComponent<SpriteRenderer>().enabled = value;
            _releasedTarget.GetComponent<SpriteRenderer>().enabled = value;
        }
    }

	public TargetLine()
	{
		_targetLine = AssetManagerLogic.instance.CreateGameObject ("Line", new Vector3 ())
			.GetComponent<LineRenderer> ();
		_targetLine.sortingLayerName = "UI";
		_targetLine.enabled = false;

        _clickDownPos = new Point2D();
		_clickUpPos = new Point2D();
		_targetLineDrawing = false;

		_selectedTarget = AssetManagerLogic.instance.CreateGameObject("Target", new Vector3 ());
		_selectedTarget.GetComponent<SpriteRenderer>().enabled = false;

		_releasedTarget = AssetManagerLogic.instance.CreateGameObject("Target", new Vector3 ());
		_releasedTarget.GetComponent<SpriteRenderer>().enabled = false;
	}

	public void DrawLine(Point2D begin, Point2D end)
	{
        _targetLine.enabled = true;
		_selectedTarget.GetComponent<SpriteRenderer>().enabled = true;
		_releasedTarget.GetComponent<SpriteRenderer>().enabled = true;

		_targetLine.SetPositions (
			new Vector3[] {
				new Vector3 (begin.x, begin.y), 
				new Vector3 (end.x, end.y) 
			});
		_selectedTarget.transform.position = new Vector3 (begin.x, begin.y);
		_releasedTarget.transform.position = new Vector3 (end.x, end.y);
	}
   
    public void MouseDown (Point2D pos)
	{
		_targetLineDrawing = true;
		_clickDownPos = pos;
		_clickUpPos = pos;

		DrawLine (pos, pos);
	}

	public void MouseMove (Point2D pos)
	{
        const int buildingRadius = 150;
		if (_targetLineDrawing) 
		{
            if (_clickDownPos.DistanceSq(pos) < buildingRadius * buildingRadius)
                return;
			_clickUpPos = pos;
			DrawLine (_clickDownPos, _clickUpPos);
		}
	}
	public void MouseUp(Point2D pos)
	{
		_targetLineDrawing = false;
	}

	private LineRenderer _targetLine;
	private Point2D _clickDownPos;
	private Point2D _clickUpPos;
	bool _targetLineDrawing;

	private GameObject _selectedTarget;
	private GameObject _releasedTarget;
}


public class HoldingBox
{
    public enum Event
    {
        NobodySelected, SomebodySelected, ButtonPressed, CommandRequested
    }

    public interface IHoldingBoxState
    {
        void MouseDown(EngineLogic engine, Point2D pos);
    }

    class MainState : IHoldingBoxState
    {
        public MainState(HoldingBox box)
        {
            _box = box;
        }

        public void MouseDown(EngineLogic engine, Point2D pos)
        {
            if (_box.Try(engine, pos))
            {
                _box.TargetLine.MouseDown(pos);
                _box._fsm.Fire(Event.SomebodySelected);
            }
            else
            {
                _box.Enable = false;
            }
        }

        HoldingBox _box;
    }

    public class SelectedState : IHoldingBoxState
    {
        public SelectedState(HoldingBox box)
        {
            _box = box;
        }

        public void MouseDown(EngineLogic engine, Point2D pos)
        {
            _box.ReleaseEntities(engine);
            if (_box.Try(engine, pos))
            {
                _box.TargetLine.MouseDown(pos);
            }
            else
            {
                _box.Enable = false;
                _box._fsm.Fire(Event.NobodySelected);
            }
        }

        HoldingBox _box;
    }

    public class CommandReadyState : IHoldingBoxState
    {
        public CommandReadyState(HoldingBox box)
        {
            _box = box;
        }

        public void MouseDown(EngineLogic engine, Point2D pos)
        {
            _box.TargetLine.DrawLine(_box.BeginPos, pos);
            _box.ProcessCommand(engine, _box.BeginPos, pos);
            _box._fsm.Fire(Event.CommandRequested);
        }

        HoldingBox _box;
    }

    public static TinyStateMachine<IHoldingBoxState, Event> CreateFSM(HoldingBox box)
    {
        var main = new MainState(box);
        var selected = new SelectedState(box);
        var commandReady = new CommandReadyState(box);

        var fsm = new TinyStateMachine<IHoldingBoxState, Event>(main);
        fsm
            .Tr(main, Event.NobodySelected, main)
            .On(() => { box.Color = Color.white; })
            //.On(()=>Debug.Log("state changed from \"MainState\" to \"MainState\"\n"))

            .Tr(main, Event.ButtonPressed, main)
            //.On(() => Debug.Log("state changed from \"MainState\" to \"MainState\"\n"))

            .Tr(main, Event.SomebodySelected, selected)
            .On(() => { box.Color = Color.white; })
            //.On(() => Debug.Log("state changed from \"MainState\" to \"SelectedState\"\n"))

            .Tr(selected, Event.NobodySelected, main)
            //.On(() => Debug.Log("state changed from \"SelectedState\" to \"MainState\"\n"))

            .Tr(selected, Event.SomebodySelected, selected)
            //.On(() => Debug.Log("state changed from \"SelectedState\" to \"SelectedState\"\n"))

            .Tr(selected, Event.ButtonPressed, commandReady)
            .On(()=>
                {
                    if (box.CurrentPressedButton == (int)EngineLogic.CommandButton.Attack)
                        box.Color = Color.red;
                    else if (box.CurrentPressedButton == (int)EngineLogic.CommandButton.Move)
                        box.Color = Color.blue;
                })
            //.On(() => Debug.Log("state changed from \"SelectedState\" to \"CommandReadyState\"\n"))

            .Tr(commandReady, Event.ButtonPressed, commandReady)
            //.On(() => Debug.Log("state changed from \"CommandReadyState\" to \"CommandReadyState\"\n"))

            .Tr(commandReady, Event.CommandRequested, main)
            //.On(() => Debug.Log("state changed from \"CommandReadyState\" to \"MainState\"\n"))
            ;

        return fsm;
    }
    
    public TinyStateMachine<IHoldingBoxState, Event> Fsm
    {
        get { return _fsm; }
    }

    public int Width
	{
		get { return _boxPoints [1].x - _boxPoints [0].x; }
	}

	public int Height
	{
		get { return _boxPoints [2].y - _boxPoints [1].y; }
	}

    public int CurrentPressedButton
    {
        get { return _currentPressedButton; }
        set { _currentPressedButton = value; }
    }

    public Point2D BeginPos
    {
        get { return _beginPos; }
    }

    public TargetLine TargetLine
    {
        get { return _targetLine; }
    }

    public Color Color
    {
        set
        {
            foreach (var l in _box)
            {
                l.startColor = value;
                l.endColor = value;
            }
            _targetLine.Color = value;
        }
    }

    public Point2D Pos
    {
        set
        {
            for (int i = 0; i < _box.Length; i++)
            {
                _box[i].SetPositions( new Vector3[] {
                    new Vector3(_boxPoints [i].x + value.x, _boxPoints[i].y + value.y, -10),
                    new Vector3(_boxPoints [(i + 1) % 4].x + value.x, _boxPoints[(i + 1) % 4].y + value.y, -10) });
            }
        }
    }

    public bool Enable
    {
        set
        {
            foreach (var b in _box)
                b.enabled = value;

            _targetLine.Enable = value;
        }
    }

    public HoldingBox()
	{
		_boxPoints = new Point2D[4]
		{
			new Point2D(-150, -150),
			new Point2D(150, -150),
			new Point2D(150, 150),
			new Point2D(-150, 150),
		};

        _box = new LineRenderer[4];
        for (int i = 0; i < _box.Length; i++)
        {
            _box[i] = AssetManagerLogic.instance.CreateGameObject("Line", new Vector3())
                .GetComponent<LineRenderer>();
            _box[i].sortingLayerName = "UI";
        }

        _idList = new List<uint>();
        _fsm = CreateFSM(this);
        _currentPressedButton = 0;
        _targetLine = new TargetLine();
        Enable = false;
        Color = Color.white;
    }


	public bool Try (EngineLogic engine, Point2D pos)
	{
        _idList.Clear();
        Pos = pos;
        Enable = true;
        _beginPos = pos;
        
        engine.World.CellSpace.CalculateNeighbors(pos, Width / 2);

        for (int i = 0; i < engine.World.CellSpace.NeighberCnt; i++)
        {
            Entity e = engine.World.CellSpace.Neighbers[i];
            if (e.PlayerId != engine.NetworkMgr.PlayerId || !(e is Character))
                continue;

            e.HandleQueried();
            _idList.Add(e.Id);
        }
        return _idList.Count > 0;
	}

    public void ReleaseEntities(EngineLogic engine)
    {
        foreach (var id in _idList)
        {
            Entity ent = engine.EntityMgr.GetEntity(id);
            if (ent != null)
            {
                ent.HandleQueried();
                ent.HandleReleaseQuery();
            }
        }
    }

    public void MouseDown(EngineLogic engine, Point2D pos)
    {
        _fsm.State.MouseDown(engine, pos);
    }


	public void ProcessCommand(EngineLogic engine, Point2D begin, Point2D end)
	{
        List<uint> newIdList = new List<uint>();
        foreach (var id in _idList) 
		{
            Entity ent = engine.EntityMgr.GetEntity(id);
            if (ent != null)
            {
                ent.HandleQueried();
                ent.HandleReleaseQuery();
                newIdList.Add(id);
            }
		}

        _idList.Clear();

        bool important = false;
        if (_currentPressedButton == (int)EngineLogic.CommandButton.Move)
        {
            important = true;
        }

        engine.InputMgr.SavedCommands.AddCommand (
			new MoveToCellCommand (
				engine.NetworkMgr.PlayerId,
                begin,
                end,
                newIdList,
                important));
	}

    private TinyStateMachine<IHoldingBoxState, Event> _fsm;
    private LineRenderer [] _box;
	private Point2D [] _boxPoints;
	private List<uint> _idList;
    private int _currentPressedButton;
    private Point2D _beginPos;
    private TargetLine _targetLine;
}


public class MinimapBox
{
    public MinimapBox()
    {
        Vector3 worldSize = Camera.main.ScreenToWorldPoint(
            new Vector3((float)Screen.width, (float)Screen.height));
        int width = (int)worldSize.x;
        int height = (int)worldSize.y;
        
        Camera mcam = Camera.main;
        int mheight = (int)mcam.orthographicSize;
        int mwidth = (int)(mcam.orthographicSize * mcam.aspect);

        _boxPoints = new Point2D[4]
        {
            new Point2D(-mwidth, -mheight),
            new Point2D(mwidth, -mheight),
            new Point2D(mwidth, mheight),
            new Point2D(-mwidth, mheight),
        };

        SetBoxRenderer(out _selectedBox, true);
        _center = AssetManagerLogic.instance.CreateGameObject("MinimapTarget", new Vector3());
        _center.transform.localScale *= 5;
    }


    public void MouseDown(EngineLogic engine, Point2D pos)
    {
        MoveBox(_selectedBox, pos);
        _center.transform.position = new Vector3(pos.x, pos.y);
    }
    
    private void SetBoxRenderer(out LineRenderer[] box, bool enable)
    {
        box = new LineRenderer[4];
        for (int i = 0; i < box.Length; i++)
        {
            box[i] = AssetManagerLogic.instance.CreateGameObject("MinimapLine", new Vector3())
                .GetComponent<LineRenderer>();
            box[i].sortingLayerName = "UI";
            box[i].enabled = enable;
        }
    }

    private void MoveBox(LineRenderer[] box, Point2D mpos)
    {
        for (int i = 0; i < box.Length; i++)
        {
            box[i].SetPositions(
                new Vector3[]
                {
                    new Vector3(_boxPoints [i].x + mpos.x, _boxPoints[i].y + mpos.y, -10),
                    new Vector3(_boxPoints [(i + 1) % 4].x + mpos.x, _boxPoints[(i + 1) % 4].y + mpos.y, -10)
                });
        }
    }

    private LineRenderer[] _selectedBox;
    GameObject _center;
    private Point2D[] _boxPoints;
}


public class InputManager
{
	public CommandList SavedCommands
	{
		get { return _savedCommands; }
	}

	public IClickable ClickDownEntity
	{
		get { return _clickDownEntity; }
	}

	public IClickable ClickUpEntity
	{
		get { return _clickUpEntity; }
	}

	public TargetLine DestinationLine
	{
		get { return _destinationLine; }
	}

    public HoldingBox HoldingBox
    {
        get { return _holdingBox; }
    }

    public MinimapBox MinimapBox
    {
        get { return _minimapBox; }
    }

    public InputManager(EngineLogic engine)
	{
		_engine = engine;
		_savedCommands = new CommandList();

		_clickables = new List<IClickable> [10];
		for (int i = 0; i < _clickables.Length; i++) 
		{
			_clickables [i] = new List<IClickable> ();
		}

		_clickDownEntity = null;
		_clickUpEntity = null;

		_holdingBox = new HoldingBox ();
		_targetLine = new TargetLine ();
		_destinationLine = new TargetLine ();
        _minimapBox = new MinimapBox();
    }

	void MouseDown(Point2D pos)
	{
		_clickDownEntity = null;
		_clickUpEntity = null;
		DeselectEntities ();
        
        _destinationLine.Enable = false;
        _targetLine.Enable = false;

        _clickDownEntity = TrySelect (pos);
		if (_clickDownEntity != null) 
		{
			_clickDownEntity.Click.HandleSelected (_engine.NetworkMgr.PlayerId);
			_targetLine.MouseDown (_clickDownEntity.Pos);
            _holdingBox.MouseDown(_engine, pos);
        }
        else
        {
			_holdingBox.MouseDown (_engine, pos);
		}
    }

	void MouseUp(Point2D pos)
	{
		_targetLine.MouseUp (pos);
		_destinationLine.Enable = false;

        // Move entities to destination
        if (_clickDownEntity == null) 
			return;
			
		_clickUpEntity = TrySelect (pos);
		if (_clickUpEntity != null) 
			_targetLine.MouseUp (_clickUpEntity.Pos);


		if (_clickDownEntity is Building) 
		{
			// When just clicked, ignore
			if (_clickUpEntity != null && _clickDownEntity.Pos == _clickUpEntity.Pos)
				return;
			
			Building building = _clickDownEntity as Building;
			_savedCommands.AddCommand (
				new CreateDestinationCommand (
					building.PlayerId, building.Id,
                    _targetLine.ClickUpPos
                    ));
		}
    }

	public void Update()
	{
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Point2D pos = new Point2D((int)worldPos.x, (int)worldPos.y);

        if (Input.GetKeyDown(KeyCode.A))
        {
            HoldingBox.CurrentPressedButton = (int)EngineLogic.CommandButton.Attack;
            HoldingBox.Fsm.Fire(HoldingBox.Event.ButtonPressed);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            HoldingBox.CurrentPressedButton = (int)EngineLogic.CommandButton.Move;
            HoldingBox.Fsm.Fire(HoldingBox.Event.ButtonPressed);
        }


        if (Input.GetAxis ("Mouse X") < 0 || Input.GetAxis ("Mouse X") > 0) 
		{
            _targetLine.MouseMove (pos);
		}
        
       
        if (_engine.UIMgr.SwallowEventOnUI(EventSystem.current.currentSelectedGameObject))
        {
            if (Input.GetMouseButtonDown(0))
                MouseDown(pos);

            else if (Input.GetMouseButtonUp(0))
                MouseUp(pos);
        }
        else if (Input.GetMouseButton(0)) { }
        
    }

	public void AddClickable(IClickable clickable, int zorder)
	{
		Debug.Assert(0 <= zorder && zorder < 10, "ZOrder max size is 9!");
		_clickables[zorder].Add (clickable);
	}

	public void RemoveClickable(IClickable clickable)
	{
		int zorder = clickable.Click.ZOrder;
		_clickables[zorder].Remove (clickable);
	}

	public IClickable TrySelect(Point2D pos)
	{
		foreach ( var clickablelist in _clickables )
		{
			foreach (var ent in clickablelist) 
			{
				if (ent.Click.HandleTrySelect (pos)) 
				{
					return ent;
				}
			}
		}
		return null;
	}

	public void DeselectEntities()
	{
		foreach (var clickablelist in _clickables) 
		{
			foreach (var ent in clickablelist) 
			{
				ent.Click.HandleDeselected ();
			}
		}
	}

    public void ClickMinimap()
    {
        Vector3 miniView = _engine.UIMgr.MinimapCamera.ScreenToViewportPoint(Input.mousePosition);
        Vector3 wpos = _engine.UIMgr.MinimapCamera.ViewportToWorldPoint(miniView);
        Vector3 worldSize = Camera.main.ScreenToWorldPoint(
            new Vector3((float)Screen.width, (float)Screen.height));

        Camera mcam = Camera.main;
        float mheight = 2f * mcam.orthographicSize;
        float mwidth = mheight * mcam.aspect;


        Camera cam = _engine.UIMgr.MinimapCamera;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        Vector3 bl = new Vector3(-width + mwidth, -height + mheight);
        Vector3 tr = new Vector3(width - mwidth, height - mheight);
        bl *= 0.5f;
        tr *= 0.5f;

        if (wpos.x < bl.x)
            wpos.x = bl.x;

        if (wpos.y < bl.y)
            wpos.y = bl.y;

        if (wpos.x > tr.x)
            wpos.x = tr.x;

        if (wpos.y > tr.y)
            wpos.y = tr.y;

        Camera.main.transform.position = wpos;
        _minimapBox.MouseDown(_engine, new Point2D((int)wpos.x, (int)wpos.y));
    }

	private EngineLogic _engine;
	private CommandList _savedCommands;

	private List<IClickable> [] _clickables;

	private IClickable _clickDownEntity;
	private IClickable _clickUpEntity;

	private HoldingBox _holdingBox;
	private TargetLine _targetLine;
	private TargetLine _destinationLine;
    private MinimapBox _minimapBox;
}
