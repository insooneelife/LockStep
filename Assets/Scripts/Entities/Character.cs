using UnityEngine;
using System.Collections;

public class Character : Entity, IHittable, IAttackable, IMovable, IAnimation
{
	public static Character CreateMelee(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		Point2D destination)
	{
		int radius = 30;
		int maxHp = 500;
		int viewRange = 450;
		int attackRange = 75;
		int damage = 25;
		int moveSpeed = 10;
		int attackFrameDelay = 10;

		// (Refactoring) Read from DB..
		if (name == "Cow") {
		} else if (name == "Oger") {
			maxHp = 1000; moveSpeed = 7;
		} else if (name == "Andariel") {
			maxHp = 1200; damage = 35; moveSpeed = 12;
		} 

		Character entity = new Character (engine, name, id, playerId, pos, (int)playerId, radius);
		entity.Hit = new Hittable(entity, maxHp);
		entity.Move = new Movable(entity, moveSpeed, destination, true);
		entity.TargetSys = new TargetSystem (entity, attackRange, viewRange);
		entity.Animate = new BaseAnimation (entity, 8);
		entity.AttackSys = new MeleeAttack (
			entity, damage, AssetManagerLogic.instance.SpriteMap [name] [entity.Animate.Action].Length / 8);
		entity.FSM = CreateFSM(entity);
		entity.Renderer = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
        entity.Color = EngineLogic.Colors[entity.PlayerId];

        engine.EntityMgr.RegisterEntity (entity);

		return entity;
	}

	public static Character CreateRange(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		Point2D destination,
		string projectileName)
	{
		int radius = 20;
		int maxHp = 500;
		int viewRange = 450;
		int attackRange = 200;
		int damage = 20;
		int moveSpeed = 8;
		int attackFrameDelay = 10;

		if (name == "Vampire")
        {
			viewRange = 350; attackRange = 250; damage = 25;
		}
        else if (name == "Wraith")
        {
			viewRange = 350; attackRange = 200; damage = 25;
		}
        else if (name == "FireGolem")
        {
			maxHp = 800; viewRange = 350; attackRange = 200; damage = 25;
		}

		Character entity = new Character (engine, name, id, playerId, pos, (int)playerId, radius);
		entity.Hit = new Hittable(entity, maxHp);
		entity.Move = new Movable(entity, moveSpeed, destination, true);
		entity.TargetSys = new TargetSystem (entity, attackRange, viewRange);
		entity.Animate = new BaseAnimation (entity, 8);
		entity.AttackSys = new RangeAttack (
			entity, damage, AssetManagerLogic.instance.SpriteMap [name] [entity.Animate.Action].Length / 8, projectileName);
		
		entity.Renderer = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
        entity.Color = EngineLogic.Colors[entity.PlayerId];

        engine.EntityMgr.RegisterEntity (entity);

        entity.FSM = CreateFSM(entity);
        return entity;
	}


	public static TinyStateMachine<IState, Event> CreateFSM(Character entity)
	{
		var patrol = new Patrol(entity, entity.TargetSys, entity.Move, entity.Animate);
		var attackToDestination = new AttackToDestination (entity, entity.TargetSys, entity.Move, entity.Animate);
		var chaseEnemy = new ChaseEnemy (entity, entity.TargetSys, entity.Move, entity.Animate);
		var attack = new Attack (entity, entity.AttackSys);
		var waitForNextAttack = new WaitForNextAttack (entity, entity.TargetSys, entity.Animate);
		var dying = new Dying (entity, entity.Animate);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(patrol);
        fsm
            .Tr(patrol, Event.HasDestination, attackToDestination)
            .On(() => entity.Animate.SetAction("Walk"))
            //.On(()=>Debug.Log("state changed from \"Patrol\" to \"AttackToDestination\"\n"))

            .Tr(patrol, Event.EnemyInView, chaseEnemy)
            .On(() => entity.Animate.SetAction("Walk"))
            //.On(()=>Debug.Log("state changed from \"Patrol\" to \"ChaseEnemy\"\n"))

            .Tr(waitForNextAttack, Event.EnemyOutOfRange, patrol)
            .On(() => entity.Animate.SetAction("Idle"))
            //.On(()=>Debug.Log("state changed from \"WaitForNextAttack\" to \"Patrol\"\n"))

            .Tr(waitForNextAttack, Event.ReadyToAttack, attack)
            .On(() => entity.Animate.SetAction("Attack"))
            //.On(()=>Debug.Log("state changed from \"WaitForNextAttack\" to \"Attack\"\n"))

            .Tr(waitForNextAttack, Event.HasDestination, attackToDestination)
            .On(() => entity.Animate.SetAction("Walk"))

            .Tr(attack, Event.DoneAttack, waitForNextAttack)
            //.On(()=>Debug.Log("state changed from \"Attack\" to \"WaitForNextAttack\"\n"))

            .Tr(attackToDestination, Event.Arrive, patrol)
            .On(() => entity.Animate.SetAction("Idle"))
            //.On(()=>Debug.Log("state changed from \"AttackToDestination\" to \"Patrol\"\n"))

            .Tr(attackToDestination, Event.EnemyInView, chaseEnemy)
            .On(() => entity.Animate.SetAction("Walk"))
            //.On(()=>Debug.Log("state changed from \"AttackToDestination\" to \"ChaseEnemy\"\n"))

            .Tr(attackToDestination, Event.HasDestination, attackToDestination)
            .On(() => entity.Animate.SetAction("Walk"))

            .Tr(chaseEnemy, Event.EnemyOutOfView, attackToDestination)
            .On(() => entity.Animate.SetAction("Walk"))
            //.On(()=>Debug.Log("state changed from \"ChaseEnemy\" to \"AttackToDestination\"\n"))

            .Tr(chaseEnemy, Event.EnemyInRange, waitForNextAttack)
            .On(() => entity.Animate.SetAction("Attack"))
            //.On(()=>Debug.Log("state changed from \"ChaseEnemy\" to \"WaitForNextAttack\"\n"))

            .Tr(chaseEnemy, Event.HasDestination, attackToDestination)
            .On(() => entity.Animate.SetAction("Walk"))

            .Tr(patrol, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"Patrol\" to \"Dying\"\n"))

            .Tr(waitForNextAttack, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"WaitForNextAttack\" to \"Dying\"\n"))

            .Tr(attack, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"Attack\" to \"Dying\"\n"))

            .Tr(chaseEnemy, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"ChaseEnemy\" to \"Dying\"\n"))

            .Tr(attackToDestination, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"AttackToDestination\" to \"Dying\"\n"))

            .Tr(dying, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))
            //.On(()=>Debug.Log("state changed from \"Dying\" to \"Dying\"\n"))

            .Tr(dying, Event.HasDestination, dying)

            .Tr(dying, Event.IsDead, dead)
            .On(() => entity.Animate.SetAction("Dead"))
            //.On(()=>Debug.Log("state changed from \"Dying\" to \"Dead\"\n"))

            .Tr(dead, Event.HasDestination, dead)
            ;

		return fsm;
	}

	// properties
	public Hittable Hit
	{
		get { return _hit; }
		set { _hit = value; }
	}

	public Movable Move
	{
		get { return _move; }
		set { _move = value; }
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

    public override bool Ignore
    {
        get { return _fsm.State is Dying || _fsm.State is Dead; }
    }

    // ctor
    public Character(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius) 
		:
	base(engine, name, id, playerId, pos, team, radius)
	{
		_queryable = new Queryable (engine.World.CellSpace, this);
	}

	public override void HandleRemove()
	{
		base.HandleRemove ();
		_queryable.HandleRemove (Engine.World.CellSpace);
        _hit.HandleRemove();
    }

	public override void HandleQueried ()
	{
        Color c = EngineLogic.Colors[PlayerId];
        c.a = c.a / 2;
        Color = c;
	}

	public override void HandleReleaseQuery ()
	{
        Color = EngineLogic.Colors[PlayerId];
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
		
	public override void Update () 
	{
		_fsm.State.Update ();
		_renderer.GetComponent<SpriteRenderer>().sortingOrder = -Pos.y;
	}

	public override void Render()
	{}
		
	private Hittable _hit;
	private Movable _move;
	private TargetSystem _targetSys;
	private AttackSystem _attackSys;
	private Animation _animate;
}


