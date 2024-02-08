using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : Entity, IHittable, ITrainable, IAnimation, IClickable, IBuilding
{
	public static Building Create(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos)
	{
		// Lets make DB and read from DB
		string[] unitDeck = new string[3];

		int radius = 150;
		int maxHp = 500;

		Building entity = new Building (
			engine,
			name,
			id,
			playerId,
			pos,
			(int)playerId,
			// Read from database..
			radius,
			maxHp,
			unitDeck);

        entity.Color = EngineLogic.Colors[entity.PlayerId];
        engine.EntityMgr.RegisterEntity (entity);

        IPhysics.EnforceByCellSpace(entity, entity.Engine.World.CellSpace,
                delegate (Entity a, Entity b)
                {
                    return b is IBuilding;
                });

        return entity;
	}

	public static TinyStateMachine<IState, Event> CreateFSM(Building entity)
	{
		var notTraining = new NotTraining(entity, entity.Animate, entity.TrainSys);
		var training = new Training(entity, entity.TrainSys);
		var dying = new Dying(entity, entity.Animate);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(notTraining);

		fsm
			.Tr(notTraining, Event.StartTraining, training)
			.Tr(training, Event.StartTraining, training)
			.Tr(training, Event.FinishTraining, notTraining)
			.Tr(notTraining, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(training, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(dying, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(dying, Event.IsDead, dead)
            .On(() => entity.Animate.SetAction("Dead"))

            .Tr(dying, Event.StartTraining, dying);
        return fsm;
	}

	// properties
	public Hittable Hit
	{
		get { return _hit; }
		set { _hit = value; }
	}

	public TrainSystem TrainSys
	{
		get { return _trainSys; }
		set { _trainSys = value; }
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

	public string [] UnitDeck
	{
		get { return _unitDeck; }
	}

    public override bool Ignore
    {
        get { return _fsm.State is Dying || _fsm.State is Dead; }
    }

    // ctor
    public Building(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius,
		int maxHp,
		string[] unitDeck) 	
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

		_unitDeck = new string[UnitDeckSize];

		_currentUnitDeckNum = (_currentUnitDeckNum + 1) % UnitDeckSize;
		_unitDeck[0] = unitSet[_currentUnitDeckNum, 0];
		_unitDeck[1] = unitSet[_currentUnitDeckNum, 1];
		_unitDeck[2] = unitSet[_currentUnitDeckNum, 2];

		_trainSys = new TrainSystem (this, pos);
        _queryable = new Queryable(engine.World.CellSpace, this);

        _renderer = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
		_renderer.GetComponent<SpriteRenderer>().sortingOrder = -pos.y;
		_fsm = CreateFSM(this);
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

	public override void SetPos(Point2D pos)
	{
        Point2D oldPos = new Point2D(Pos.x, Pos.y);
        base.SetPos (pos);
		_hit.SetPos (pos);
        _queryable.UpdateCellSpace(_engine.World.CellSpace, oldPos);
    }
		
	public override void Update () 
	{
		_fsm.State.Update ();
	}

	public override void Render()
	{}

	public void EventOnSelected(uint playerId)
	{
		_engine.UIMgr.ShowInfomation (true, Name, _hit.Hp, 0);
		if (this.PlayerId != playerId)
			return;
		
		_engine.InputMgr.DestinationLine.DrawLine (Pos, _trainSys.Destination);
		_engine.UIMgr.ShowTrainUnitList (true, _unitDeck);
	}

	public void EventOnDeselected()
	{
		_engine.UIMgr.ShowInfomation (false);
		_engine.UIMgr.ShowTrainUnitList (false);
	}

	public void EventOnTrySelected(bool onOff)
	{}

	const int UnitDeckSize = 3;
	static readonly string[,] unitSet = new string[,]
	{ 
		{"Cow", "Oger", "Andariel"}, 
		{"Vampire", "Wraith", "FireGolem"}, 
		{"SoulStone", "Izual", "Izual"}
	};

	static int _currentUnitDeckNum = 0;

	private Hittable _hit;
	private TrainSystem _trainSys;
	private Animation _animate;
	private Clickable _clickable;

	private string [] _unitDeck;
}