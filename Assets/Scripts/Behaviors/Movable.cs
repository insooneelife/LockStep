using UnityEngine;
using System.Collections;

public class Movable 
{
	public int MoveSpeed 
	{
		get { return _moveSpeed; } 
		set{ _moveSpeed = value; }
	}

	public Point2D Destination
	{
		get{ return _destination; }
		set{ _destination = value; }
	}

	public bool HasDestination
	{
		get { return _hasDestination; }
		set { _hasDestination = value; }
	}

    public bool Important
    {
        get { return _important; }
        set { _important = value; }
    }


    public Movable(Entity entity, int moveSpeed, Point2D destination, bool hasDestination)
	{
		Debug.Assert (entity is IMovable, "entity is not IMovable!");

		_owner = entity;
		_moveSpeed = moveSpeed;
		_destination = destination;
		_hasDestination = hasDestination;
        _important = false;

    }

	public void UpdateMovement()
	{
		Point2D oldPos = new Point2D (_owner.Pos.x, _owner.Pos.y);
		
		int length = _owner.Pos.DistanceSq (_destination) + 1;
        Point2D vec = new Point2D(_destination - _owner.Pos);

        vec *= MoveSpeed;
        vec /= IMath.Sqrt(length);

        _owner.Direction = Utils.MakeDirection (vec.x, vec.y);
		_owner.SetPos (_owner.Pos + vec);

		if(_owner.Queryable != null)
		{        
            _owner.Queryable.UpdateCellSpace (_owner.Engine.World.CellSpace, oldPos);
		}
	}

	Entity _owner;
	private int _moveSpeed;
	private Point2D _destination;
	private bool _hasDestination;
    private bool _important;
}
