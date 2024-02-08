using UnityEngine;
using System.Collections;

public class Projectile : Entity, IMovable
{
	public static Projectile Create(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		uint targetId,
		int damage)
	{
		int radius = 30;
		int impactRange = 20;
		int moveSpeed = 12;

		Projectile entity = new Projectile (
			engine,
			name,
			id,
			playerId,
			pos,
			(int)playerId,
			radius,
			targetId,
			impactRange,
			damage,
			moveSpeed);
		
		engine.EntityMgr.RegisterEntity (entity);

		return entity;
	}

	public static TinyStateMachine<IState, Event> CreateRangeFSM(Projectile entity)
	{
		var flyToEntity = new FlyToEntity(entity, entity.Move);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(flyToEntity);

		fsm
			.Tr(flyToEntity, Event.Arrive, dead)
		    //.On(() => Debug.Log("Attack Range"))
            ;
		
		return fsm;
	}

	public static TinyStateMachine<IState, Event> CreateSingleFSM(Projectile entity)
	{
		var flyToEntity = new FlyToEntity(entity, entity.Move);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(flyToEntity);
		fsm
			.Tr (flyToEntity, Event.Arrive, dead)
            //.On(() => Debug.Log("Attack Single"))
            ;

        return fsm;
	}

	// properties
	public uint TargetId
	{
		get { return _targetId; }
	}

	public int ImpactRange
	{
		get { return _impactRange; }
	}

	public int Damage
	{
		get { return _damage; }
	}

	public Movable Move
	{
		get { return _move; }
		set { _move = value; }
	}

	// ctor
	public Projectile(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius,
		uint targetId,
		int impactRange,
		int damage, 
		int moveSpeed) 
		:
	base(engine, name, id, playerId, pos, team, radius)
	{
		_targetId = targetId;
		_impactRange = impactRange;
		_damage = damage;

		Entity target = Engine.EntityMgr.GetEntity (TargetId);
		Point2D destination;
		if (target == null) 
			destination = Pos;
		else 
			destination = target.Pos;

		_move = new Movable (this, moveSpeed, destination, true);
        _renderer = null;
        _particle = AssetManagerLogic.instance.CreateGameObject(name, new Vector3 (pos.x, pos.y));
        _fsm = CreateSingleFSM(this);
	}

	// set position with sprite
	public override void SetPos(Point2D pos)
	{ 
		base.SetPos (pos);
		_particle.transform.position = new Vector3 (pos.x, pos.y);
	}

	public override void HandleRemove()
	{
		base.HandleRemove ();
        UnityEngine.Object.Destroy(_particle, 3);
    }

	public override void Update () 
	{
		_fsm.State.Update ();
	}

	public override void Render()
	{}

	public void Stop ()
	{
		_particle.GetComponent<ParticleSystem>().Stop ();
	}

	private uint _targetId;
	private int _impactRange;
	private int _damage;

	private GameObject _particle;
	private Movable _move;
}
