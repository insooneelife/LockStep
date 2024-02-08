using UnityEngine;
using System.Collections;

public interface IMessageHandler
{
	bool HandleMessage(Message msg);
}

public interface IAnimation
{
	Animation Animate { get; set; }
}

public interface IHittable : IMessageHandler, IAnimation
{
	Hittable Hit { get; set; }
}

public interface ITargetable
{ 
	TargetSystem TargetSys { get; set; } 
}

public interface IAttackable : ITargetable, IMessageHandler
{
	AttackSystem AttackSys { get; set; }
}

public interface IMovable
{
	Movable Move { get; set; }
	Point2D Pos { get; }
}

public interface ITrainable
{
	TrainSystem TrainSys { get; set; }
}

public interface IClickable
{
	Clickable Click { get; set; }
	Point2D Pos { get; }
    Color Color { get; set; }
    uint PlayerId { get; }

	bool Collide (Point2D clickedPos);
	void EventOnSelected (uint playerId);
	void EventOnDeselected();
	void EventOnTrySelected(bool onOff);
}


public interface IBuilding
{ }