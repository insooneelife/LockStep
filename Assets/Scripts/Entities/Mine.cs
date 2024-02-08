using UnityEngine;
using System.Collections;

public class Mine : Entity, IHittable, IAnimation, IClickable, IBuilding
{
	public static Mine Create(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos)
	{
		int radius = 75;
		int maxHp = 500;
		int earnGold = 5;

		Mine entity = new Mine (
			engine,
			name,
			id,
			playerId,
			pos,
			(int)playerId,
			// Read from database..
			radius,
			maxHp,
			earnGold);

        entity.Color = EngineLogic.Colors[entity.PlayerId];
        engine.EntityMgr.RegisterEntity (entity);

        IPhysics.EnforceByCellSpace(entity, entity.Engine.World.CellSpace,
                delegate (Entity a, Entity b)
                {
                    return b is IBuilding;
                });

        return entity;
	}

	public static TinyStateMachine<IState, Event> CreateFSM(Mine entity)
	{
		var idle = new Idle(entity, entity.Animate);
		var dying = new Dying(entity, entity.Animate);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(idle);

		fsm
			.Tr (idle, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr (dying, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr (dying, Event.IsDead, dead)
            .On(() => entity.Animate.SetAction("Dead"))
            ;

		return fsm;
	}

	// properties
	public Hittable Hit
	{
		get { return _hit; }
		set { _hit = value; }
	}

	public Animation Animate
	{
		get { return _animate; }
		set { _animate = value; }
	}

	public Clickable Click 
	{
		get { return _clickable; }
		set { _clickable = value; }
	}

    public override bool Ignore
    {
        get { return _fsm.State is Dying || _fsm.State is Dead; }
    }

    // ctor
    public Mine(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius,
		int maxHp,
		int earnGold) 	
		: 
	base(engine, name, id, playerId, pos, team, radius)
	{
		_hit = new Hittable(this, maxHp);
		_animate = new NoneAnimation (this, 1);
		_clickable = new Clickable (
			_engine.InputMgr, 
			this,
			EventOnSelected,
			EventOnDeselected,
			EventOnTrySelected,
			null,
			0);
		
		_earnGold = earnGold;
		_earnGoldFrame = 0;
		_earnGoldDelay = 200;

		_renderer = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
		_renderer.GetComponent<SpriteRenderer>().sortingOrder = -pos.y;

        _queryable = new Queryable(engine.World.CellSpace, this);
        _particle = AssetManagerLogic.instance.CreateGameObject("GainGold", new Vector3 (pos.x, pos.y));
		_fsm = CreateFSM(this);
	}

	// set position with sprite
	public override void SetPos(Point2D pos)
	{ 
        Point2D oldPos = new Point2D(Pos.x, Pos.y);
		base.SetPos (pos);
		_hit.SetPos (pos);
        _queryable.UpdateCellSpace(_engine.World.CellSpace, oldPos);        
    }

	public override bool HandleMessage(Message msg)
	{
		int damage = 0;
		if (msg.Msg == Message.MsgType.Damage)
		{
			damage = (int)msg.ExtraInfo;
			Hit.HasDamaged(damage);
			return true;
		}
		return false;
	}

	public override void HandleRemove()
	{
		base.HandleRemove ();
		_clickable.HandleRemove (_engine.InputMgr);
        _hit.HandleRemove();
        _queryable.HandleRemove(Engine.World.CellSpace);
    }

	public override void Update () 
	{
		_fsm.State.Update ();

		_earnGoldFrame++;
		if (_earnGoldFrame % _earnGoldDelay == 0) 
		{
			_earnGoldFrame = 0;
			_engine.UIMgr.SetGold (_engine.UIMgr.Gold + _earnGold);

			_particle.GetComponent<ParticleSystem>().Play ();
		}
	}

	public override void Render()
	{}


	public void EventOnSelected(uint playerId)
	{
		_engine.UIMgr.ShowInfomation (true, Name, _hit.Hp, 0);
	}

	public void EventOnDeselected()
	{
		_engine.UIMgr.ShowInfomation (false);
	}

	public void EventOnTrySelected(bool onOff)
	{}
		
	private Hittable _hit;
	private Animation _animate;
	private Clickable _clickable;

	private GameObject _particle;

	private int _earnGold;
	private int _earnGoldFrame;
	private int _earnGoldDelay;
}