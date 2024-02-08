using UnityEngine;
using System.Collections;

public abstract class AttackSystem
{
	public int Damage 
	{
		get { return _damage; }
		set { _damage = value; }
	}

	public int AttackFrame 
	{
		get { return _attackFrame; }
		set { _attackFrame = value; }
	}

	public int AttackFrameDelay 
	{
		get { return _attackFrameDelay; }
		set { _attackFrameDelay = value; }
	}

	public AttackSystem(Entity entity, int damage, int attackFrameDelay)
	{
		Debug.Assert (entity is IAttackable, "entity is not IAttackable!");

		_owner = entity;
		_targetSys = (entity as ITargetable).TargetSys;

		_damage = damage;
		_attackFrame = 0;
		_attackFrameDelay = attackFrameDelay;
	}

	public abstract void UpdateAttack ();

	protected Entity _owner;
	protected TargetSystem _targetSys;

	private int _damage;
	private int _attackFrame;
	private int _attackFrameDelay;
}


public class MeleeAttack : AttackSystem
{
	public MeleeAttack(Entity entity, int damage, int attackFrameDelay)
		:
	base(entity, damage, attackFrameDelay)
	{}

	public override void UpdateAttack()
	{
		_owner.Engine.EntityMgr
			.DispatchMsg (_owner.Id, _targetSys.TargetId, Message.MsgType.Damage, Damage);
	}
}

public class RangeAttack : AttackSystem
{
	public RangeAttack(Entity entity, int damage, int attackFrameDelay, string projectileName)
		:
	base(entity, damage, attackFrameDelay)
	{
		_projectileName = projectileName;
	}

	public override void UpdateAttack()
	{
        _owner.Engine.World.CreateProjectile(_projectileName, _owner.PlayerId, _owner.Pos, _targetSys.TargetId, Damage);
    }

	private string _projectileName;
}