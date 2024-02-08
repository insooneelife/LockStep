using UnityEngine;
using System.Collections;

public abstract class Animation
{
	public string Action
	{
		get { return _action; }
		set { _action = value; }
	}

	public int Frame
	{
		get { return _frame; }
		set { _frame = value; }
	}

	public int MaxFrame
	{
		get { return _maxFrame; }
		set { _maxFrame = value; }
	}

	public Animation(Entity entity, int maxDirections)
	{
		Debug.Assert (entity is IAnimation, "entity is not IAnimation!");

		_owner = entity;
		_frame = 0;
		_maxFrame = 1;
		_MaxDirections = maxDirections;
	}

	public bool FrameCount()
	{
		_frame++;
		if (_frame % _maxFrame == 0)
		{
			_frame = 0;
			return true;
		}
		return false;
	}

	public abstract void SetAction (string action);

	public abstract bool UpdateAnimation (SpriteRenderer sprite);

	protected Entity _owner;
	protected string _action;
	protected int _frame;
	protected int _maxFrame;

	protected readonly int _MaxDirections;
}


public class BaseAnimation : Animation
{
	public BaseAnimation(Entity entity, int maxDirections)
		:base(entity, maxDirections)
	{
		SetAction ("Idle");
	}

	public override void SetAction(string action)
	{
		_frame = 0;
		_action = action;
		_maxFrame = AssetManagerLogic.instance.SpriteMap [_owner.Name] [_action].Length / _MaxDirections;
	}

	public override bool UpdateAnimation(SpriteRenderer sprite)
	{
		sprite.sprite = AssetManagerLogic.instance
			.SpriteMap [_owner.Name] [_action] [(_maxFrame * _owner.Direction) + _frame];

		return FrameCount ();
	}
}

public class NoneAnimation : Animation
{
	public NoneAnimation(Entity entity, int maxDirections)
		:base(entity, maxDirections)
	{}

	public override void SetAction(string action)
	{}

	public override bool UpdateAnimation(SpriteRenderer sprite)
	{
		return true;
	}
}