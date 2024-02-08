using UnityEngine;
using System.Collections;

public abstract class Entity : IMessageHandler, ICellSpaceQueryable
{
	public enum Event
	{
		EnemyInRange, EnemyOutOfRange, ReadyToAttack, DoneAttack,
		HasToDie, IsDead,
		HasDestination, EnemyInView, EnemyOutOfView,
		StartTraining, FinishTraining,
		Arrive
	}
		
	public EngineLogic Engine
	{
		get{ return _engine; }
	}

	public string Name
	{
		get{ return _name; }
	}

	public uint Id
	{
		get{ return _id; }
	}

	public uint PlayerId
	{
		get{ return _playerId; }
	}

	// Because of using unity Sprite, set position only with SetPos() method.
	public Point2D Pos
	{ 
		get { return _pos; }
	}

	public int Team
	{
		get { return _team; }
	}

	public TinyStateMachine<IState, Event> FSM
	{
		get { return _fsm; }
		set { _fsm = value; }
	}

	public GameObject Renderer
	{
		get { return _renderer; }
		set { _renderer = value; }
	}

    public SpriteRenderer SpriteRenderer
    {
        get { return _renderer.GetComponent<SpriteRenderer>(); }
    }

    public Color Color
    {
        get
        {
            if (_renderer == null)
                return Color.white;
            return _renderer.GetComponent<SpriteRenderer>().color;
        }
        set
        {
            if (_renderer != null)
                _renderer.GetComponent<SpriteRenderer>().color = value;
        }
    }

	public Queryable Queryable
	{
		get { return _queryable; }
		set { _queryable = value; }
	}
	
    public virtual bool Ignore
    {
        get { return _dead; }
    }

	public bool Dead
	{
		get { return _dead; }
		set 
		{
            if(_renderer)
			    _renderer.GetComponent<SpriteRenderer>().enabled = false;
			_dead = value; 
		}
	}
		
	public int Radius
	{
		get { return _radius;}
	}

	public int Direction
	{
		get { return _direction; }
		set { _direction = value; } 
	}
    
	// Constructor
	public Entity(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius)
	{
		_engine = engine;
		_name = name;
		_id = id;
		_playerId = playerId;
		_pos = pos;
		_team = team;

		_skipCnt = 0;

		_radius = radius;
		_direction = 0;

		_dead = false;

		// Must initialized in child class.
		_fsm = null;
		_renderer = null;
		_queryable = null;
	}

	// Set position with sprite
	public virtual void SetPos(Point2D pos)
	{ 
		_pos = pos;
        if(_renderer)
		    _renderer.transform.position = new Vector3 (pos.x, pos.y, 0);
	}

	public bool Collide(Point2D pos)
	{
		int dis2 = IMath.Dis2 (_pos.x, _pos.y, pos.x, pos.y);
		return dis2 <= _radius * _radius;
	}

	public bool IsTargetPresent(uint id)
	{
		return _engine.EntityMgr.Exists (id);
	}

	// Update is called once per frame
	public abstract void Update ();
	public abstract void Render ();

	public virtual void HandleRemove()
	{
		_engine.EntityMgr.UnRegisterEntity (this);
		UnityEngine.Object.Destroy (_renderer);
	}

	public virtual void HandleQueried ()
	{}

	public virtual void HandleReleaseQuery ()
	{}
	
	public bool Skip(int skipNum)
	{
		_skipCnt++;
		if (_skipCnt % skipNum == 0)
		{
			_skipCnt = 0;
			return true;
		}
		return false;
	}

	public virtual bool HandleMessage(Message msg)
	{
		return false;
	}


	protected EngineLogic _engine;
	private string _name;
	private uint _id;
	private uint _playerId;
	private Point2D _pos;
	private int _team;

	protected TinyStateMachine<IState, Event> _fsm;
	protected GameObject _renderer;
    protected Queryable _queryable;

	private bool _dead;

	private int _skipCnt;

	private int _radius;
	private int _direction;
}
