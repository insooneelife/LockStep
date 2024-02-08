using UnityEngine;
using System.Collections;

public class TargetSystem
{
	public uint TargetId 
	{
		get { return _targetId; }
		set { _targetId = value;}
	}

	public bool Attackable 
	{ 
		get { return _attackAble; }
		set { _attackAble = value;} 
	}

	public bool Viewable 
	{
		get { return _viewAble; }
		set { _viewAble = value; }
	}

	public int ViewRange 
	{
		get { return _viewRange; }
	}

	public int AttackRange 
	{
		get { return _attackRange; }
	}

	public TargetSystem(
		Entity entity,
		int attackRange,
		int viewRange)
	{
		Debug.Assert (entity is ITargetable, "entity is not ITargetable!");

		_entity = entity;
		_targetId = EntityManager.InvalidateId;
		_attackAble = false;
		_viewAble = false;
		_attackRange = attackRange;
		_viewRange = viewRange;
	}

	public virtual Entity UpdateTarget()
	{
		Entity targetEnt = _entity.Engine.EntityMgr.GetEntity(TargetId);

		// If target presents..
		if (targetEnt != null && !targetEnt.Ignore)
		{
			Attackable = IMath.InRange(_entity.Pos, targetEnt.Pos, AttackRange + targetEnt.Radius);
			Viewable = IMath.InRange(_entity.Pos, targetEnt.Pos, ViewRange + targetEnt.Radius);
			return targetEnt;
		}
		// Otherwise, we have to set a new target.
		else
        {
            int distanceSq = -1;
			targetEnt = _entity.Engine.World.ClosestEntityFromPos(
               _entity, _viewRange, out distanceSq, delegate(Entity e, Entity target)
                {
                    return e.Team == target.Team || target.Ignore;
                });

			if (targetEnt == null) 
			{
				Attackable = false;
				Viewable = false;
				TargetId = EntityManager.InvalidateId;
				return null;
			}

			if (distanceSq < IMath.Square(ViewRange))
			{
				TargetId = targetEnt.Id;
				Attackable = IMath.InRange(_entity.Pos, targetEnt.Pos, AttackRange + targetEnt.Radius);
				Viewable = IMath.InRange(_entity.Pos, targetEnt.Pos, ViewRange + targetEnt.Radius);
				return targetEnt;
			}
			else
			{
				Attackable = false;
				Viewable = false;
				TargetId = EntityManager.InvalidateId;
				return null;
			}
		}
	}

	bool IsTargetPresent()
	{
		return _entity.Engine.EntityMgr.Exists (_targetId);
	}

	private Entity _entity;
	private uint _targetId;
	private bool _attackAble;
	private bool _viewAble;
	private int _attackRange;
	private int _viewRange;
}
