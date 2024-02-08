using UnityEngine;
using System.Collections.Generic;

public class Builder : Entity, IHittable, IMovable, IAnimation
{
	public static Builder Create(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		Point2D destination,
        List<KeyValuePair<int, string>> relatedEntities)
	{
		int radius = 25;
		int maxHp = 500;
		int moveSpeed = 10;

        var relation = relatedEntities.Find(x => x.Key == (int)World.Relation.CreateByBuild);
        string buildName = relation.Value;

        Builder entity = new Builder (engine, name, id, playerId, pos, (int)playerId, radius, buildName);
		entity.Hit = new Hittable(entity, maxHp);
		entity.Move = new Movable(entity, moveSpeed, destination, true);
		entity.Animate = new BaseAnimation (entity, 8);
		entity.Renderer = AssetManagerLogic.instance.CreateGameObject (name, new Vector3 (pos.x, pos.y));
        entity.Color = EngineLogic.Colors[entity.PlayerId];

        engine.EntityMgr.RegisterEntity (entity);
        entity.FSM = CreateFSM(entity);
        return entity;
	}


	public static TinyStateMachine<IState, Event> CreateFSM(Builder entity)
	{
		var buildTarget = new BuildTarget (entity, entity.Animate, entity.BuildName);
		var walkForBuild = new WalkForBuild(entity,entity.Move, entity.Animate);
		var dying = new Dying (entity, entity.Animate);
		var dead = new Dead(entity);

		var fsm = new TinyStateMachine<IState, Event>(walkForBuild);
        fsm
            .Tr(walkForBuild, Event.Arrive, buildTarget)
            .On(() => entity.Animate.SetAction("Idle"))

            .Tr(walkForBuild, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(buildTarget, Event.HasToDie, dying)
            .On(() => entity.Animate.SetAction("Dying"))

            .Tr(buildTarget, Event.IsDead, dead)
            .On(() => entity.Animate.SetAction("Dead"))

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

	public Movable Move
	{
		get { return _move; }
		set { _move = value; }
	}

	public Animation Animate
	{
		get { return _animate; }
		set { _animate = value; }
	}

	public string BuildName
	{
		get { return _buildName; }
		set { _buildName = value; }
	}

    public override bool Ignore
    {
        get { return _fsm.State is Dying || _fsm.State is Dead; }
    }

    // ctor
    public Builder(
		EngineLogic engine,
		string name,
		uint id,
		uint playerId,
		Point2D pos,
		int team,
		int radius,
		string buildName) 
		:
	base(engine, name, id, playerId, pos, team, radius)
	{
		_queryable = new Queryable (engine.World.CellSpace, this);
		_buildName = buildName;
	}

	public override void HandleRemove()
	{
		base.HandleRemove ();
		_queryable.HandleRemove (Engine.World.CellSpace);
        _hit.HandleRemove();

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
	private Animation _animate;
	private string _buildName;
}


