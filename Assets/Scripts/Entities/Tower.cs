using UnityEngine;
using System.Collections;

public class Tower : Entity, IHittable, IAttackable, IAnimation, IClickable, IBuilding
{
	public static Tower Create(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		string projectileName)
	{
		int radius = 80;
		int maxHp = 1200;
		int viewRange = 450;
		int attackRange = 450;
		int damage = 80;
		int attackFrameDelay = 20;

		Tower entity = new Tower (
			engine,
			name,
			id,
			playerId,
			pos,
			(int)playerId,
			radius,
			maxHp,
			viewRange,
			attackRange,
			damage,
			attackFrameDelay,
			projectileName);

        entity.Color = EngineLogic.Colors[entity.PlayerId];
        engine.EntityMgr.RegisterEntity (entity);

        IPhysics.EnforceByCellSpace(entity, entity.Engine.World.CellSpace,
                delegate (Entity a, Entity b)
                {
                    return b is IBuilding;
                });

        return entity;
	}

	public static TinyStateMachine<IState, Event> CreateFSM(Tower entity)
	{
		var holdPosition = new HoldPosition (entity, entity.TargetSys, entity.Animate);
		var attack = new Attack (entity, entity.AttackSys);
		var waitForNextAttack = new WaitForNextAttack (entity, entity.TargetSys, entity.Animate);
		var dying = new Dying (entity, entity.Animate);
		var dead = new Dead (entity);
        
        var fsm = new TinyStateMachine<IState, Event>(holdPosition);
        fsm
            .Tr(holdPosition, Event.EnemyInRange, attack)
            .On(() => entity.Animate.SetAction("Attack"))

			.Tr(attack, Event.DoneAttack, waitForNextAttack)

			.Tr(waitForNextAttack, Event.ReadyToAttack, attack)
            .On(() => entity.Animate.SetAction("Attack"))

            .Tr(waitForNextAttack, Event.EnemyOutOfRange, holdPosition)
            .On(() => entity.Animate.SetAction("Idle"))

            .Tr(attack, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(holdPosition, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(waitForNextAttack, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(dying, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(dying, Event.IsDead, dead)
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

	public TargetSystem TargetSys
	{
		get { return _targetSys; }
		set { _targetSys = value; }
	}

	public AttackSystem AttackSys
	{
		get { return _attackSys; }
		set { _attackSys = value; }
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
    public Tower(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius,
		int maxHp,
		int viewRange,
		int attackRange,
		int damage,
		int attackFrameDelay,
		string projectileName) 
		:
	base(engine, name, id, playerId, pos, team, radius)
	{
		_hit = new Hittable (this, maxHp);
		_targetSys = new TargetSystem (this, attackRange, viewRange);
		_attackSys = new RangeAttack (this, damage, attackFrameDelay, projectileName);
		_animate = new BaseAnimation (this, 1);
		_clickable = new Clickable (
			_engine.InputMgr,
			this,
			EventOnSelected,
			EventOnDeselected,
			EventOnTrySelected,
			null,
			0);
        
		_renderer = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
		_renderer.GetComponent<SpriteRenderer>().sortingOrder = -pos.y;
        _queryable = new Queryable(engine.World.CellSpace, this);
		_fsm = CreateFSM(this);
    }

	// Set position with sprite
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

    }

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

	public override void Update () 
	{
		_fsm.State.Update ();
	}

	public override void Render()
	{}
		
	private Hittable _hit;
	private TargetSystem _targetSys;
	private AttackSystem _attackSys;
	private Animation _animate;
	private Clickable _clickable;
}
