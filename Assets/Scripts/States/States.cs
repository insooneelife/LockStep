using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface IState
{
	void Update();
}

public class HoldPosition : IState
{
	public HoldPosition(Entity entity, TargetSystem targetSys, Animation animate)
	{
		_owner = entity;
		_targetSys = targetSys;
		_animate = animate;
	}

	public void Update()
	{
		if (!_owner.Skip (8))
			return;

		_animate.UpdateAnimation(_owner.SpriteRenderer);

		_targetSys.UpdateTarget ();
		if (_targetSys.Attackable)
		{
			_owner.FSM.Fire(Entity.Event.EnemyInRange);
		}
	}
		
	private Entity _owner;
	private TargetSystem _targetSys;
	private Animation _animate;
}

public class Attack : IState
{
	public Attack(Entity entity, AttackSystem attackSys)
	{
		_owner = entity;
		_attackSys = attackSys;
	}

	public void Update()
	{
		_attackSys.UpdateAttack ();
		_owner.FSM.Fire(Entity.Event.DoneAttack);
	}

	private Entity _owner;
	private AttackSystem _attackSys;
}


public class WaitForNextAttack : IState
{
	public WaitForNextAttack(Entity entity, TargetSystem targetSys, Animation animate)
	{
		_owner = entity;
		_targetSys = targetSys;
		_animate = animate;
	}

	public void Update()
	{
		if (!_owner.Skip (3))
			return;
		
		if (_animate.UpdateAnimation(_owner.SpriteRenderer))
		{
			_targetSys.UpdateTarget ();
			if (_targetSys.Attackable)
			{
				_owner.FSM.Fire(Entity.Event.ReadyToAttack);
			}
			else
			{
				_owner.FSM.Fire(Entity.Event.EnemyOutOfRange);
			}
		}
	}

	private Entity _owner;
	private TargetSystem _targetSys;
	private Animation _animate;
}


public class Dying : IState
{
	public Dying(Entity entity, Animation animate)
	{
		_owner = entity;
		_animate = animate;
	}

	public void Update()
	{
		if (!_owner.Skip (3))
			return;

		if (_animate.UpdateAnimation(_owner.SpriteRenderer))
		{
			_owner.FSM.Fire(Entity.Event.IsDead);
			return;
		}
	}

	private Entity _owner;
	private Animation _animate;
}

public class Dead : IState
{
	public Dead(Entity entity)
	{
		_owner = entity;
	}

	public void Update()
	{
		if (!_owner.Skip (60))
			return;
		
		_owner.Dead = true;
	}

	private Entity _owner;
}


public class NotTraining : IState
{
	public NotTraining(Entity entity, Animation animate, TrainSystem trainSys)
	{
		_owner = entity;
		_animate = animate;
		_trainSys = trainSys;
	}

	public void Update()
	{
		_animate.UpdateAnimation(_owner.SpriteRenderer);

		if (_trainSys != null && _trainSys.TrainQCount > 0) 
		{
            var data = _trainSys.TrainQDequeue();

            _trainSys.SetNextTraining (data);
			_owner.FSM.Fire (Entity.Event.StartTraining);
		}
	}

	private Entity _owner;
	private Animation _animate;
	private TrainSystem _trainSys;
}


public class Training : IState
{
	public Training(Entity entity, TrainSystem trainSys)
	{
		_owner = entity;
		_trainSys = trainSys;
	}

	public void Update()
	{
		_owner.SpriteRenderer.color = Color.blue;
		if(_trainSys.TrainDelayFinished())
		{
			_trainSys.TrainEntity ();
			_owner.FSM.Fire(Entity.Event.FinishTraining);
			_owner.Color = EngineLogic.Colors[_owner.PlayerId];
		}
	}

	private Entity _owner;
	private TrainSystem _trainSys;
}

public class Idle : IState
{
	public Idle(Entity entity, Animation animate)
	{
		_owner = entity;
		_animate = animate;
	}

	public void Update()
	{
		_animate.UpdateAnimation(_owner.SpriteRenderer);
	}

	private Entity _owner;
	private Animation _animate;
}

public class FlyToEntity : IState
{
	public FlyToEntity(Projectile entity, Movable move)
	{
		_owner = entity;
		_move = move;
	}

	public void Update()
	{
		if (IMath.InRange(_move.Destination, _owner.Pos, _owner.Radius))
		{
			_owner.Engine.EntityMgr
				.DispatchMsg (_owner.Id, _owner.TargetId, Message.MsgType.Damage, _owner.Damage);
			_owner.Stop ();
			_owner.FSM.Fire(Entity.Event.Arrive);
		}
			
		_move.UpdateMovement ();
	}

	private Projectile _owner;
	private Movable _move;
}


public class Patrol : IState
{
	public Patrol(Entity entity, TargetSystem targetSys, Movable move, Animation animate)
	{
		_owner = entity;
		_targetSys = targetSys;
		_move = move;
		_animate = animate;
	}

	public void Update()
	{
		if (_move.HasDestination)
		{
			_owner.FSM.Fire(Entity.Event.HasDestination);
			return;
		}

		Entity target = _targetSys.UpdateTarget ();
		if (_targetSys.Viewable)
		{
			_move.Destination = target.Pos;
			_owner.FSM.Fire(Entity.Event.EnemyInView);
		}

		if (!_owner.Skip (3))
			return;
		
		_animate.UpdateAnimation(_owner.SpriteRenderer);
	}

	private Entity _owner;
	private TargetSystem _targetSys;
	private Movable _move;
	private Animation _animate;
}


public class AttackToDestination : IState
{
	public AttackToDestination(Entity entity, TargetSystem targetSys, Movable move, Animation animate)
	{ 
		_owner = entity;
		_targetSys = targetSys;
		_move = move;
		_animate = animate;
	}

	public void Update()
	{
		int arriveExpected = 50 * 50;
		if (_move.Destination.DistanceSq(_owner.Pos) < arriveExpected)
		{
			_move.HasDestination = false;
			_owner.FSM.Fire(Entity.Event.Arrive);
			return;
		}

        if (!_move.Important)
        {
            Entity target = _targetSys.UpdateTarget();

            if (_targetSys.Viewable)
            {
                _move.Destination = target.Pos;
                _owner.FSM.Fire(Entity.Event.EnemyInView);
                return;
            }
        }

		if (!_owner.Skip (2))
			return;

		_animate.UpdateAnimation(_owner.SpriteRenderer);
		_move.UpdateMovement ();
	}

	private Entity _owner;
	private TargetSystem _targetSys;
	private Movable _move;
	private Animation _animate;
}


public class ChaseEnemy : IState
{
	public ChaseEnemy(Entity entity, TargetSystem targetSys, Movable move, Animation animate)
	{
		_owner = entity;
		_targetSys = targetSys;
		_move = move;
		_animate = animate;
	}

	public void Update()
	{
		Entity target = _targetSys.UpdateTarget ();

		if (_targetSys.Attackable) 
		{
			_owner.FSM.Fire(Entity.Event.EnemyInRange);
			return;
		}
			
		if(!_targetSys.Viewable)
		{
			_owner.FSM.Fire(Entity.Event.EnemyOutOfView);
			return;
		}
			
		if (_targetSys.Viewable)
			_move.Destination = target.Pos;

		if (!_owner.Skip (3))
			return;

		_animate.UpdateAnimation(_owner.SpriteRenderer);
		_move.UpdateMovement ();
	}

	private Entity _owner;
	private TargetSystem _targetSys;
	private Movable _move;
	private Animation _animate;
}


public class WalkForBuild : IState
{
	public WalkForBuild(Entity entity, Movable move, Animation animate)
	{ 
		_owner = entity;
		_move = move;
		_animate = animate;
		_animate.SetAction ("Walk");
	}

	public void Update()
	{
		int arriveExpected = 30 * 30;
		if (_move.Destination.DistanceSq(_owner.Pos) < arriveExpected)
		{
			_move.HasDestination = false;
			_owner.FSM.Fire(Entity.Event.Arrive);
			return;
		}

		if (!_owner.Skip (2))
			return;

		_animate.UpdateAnimation(_owner.SpriteRenderer);
		_move.UpdateMovement ();
	}

	private Entity _owner;
	private Movable _move;
	private Animation _animate;
}

public class BuildTarget : IState
{
	public BuildTarget(Entity entity, Animation animate, string buildTarget)
	{ 
		_owner = entity;
		_animate = animate;
		_buildTarget = buildTarget;
	}

	public void Update()
	{
        _owner.Engine.World.CreateBuilding(_buildTarget, _owner.PlayerId, _owner.Pos);
		_owner.FSM.Fire(Entity.Event.IsDead);
	}

	private Entity _owner;
	private Animation _animate;
	private string _buildTarget;
}