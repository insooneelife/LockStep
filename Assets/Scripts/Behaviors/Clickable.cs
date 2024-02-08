using UnityEngine;
using System.Collections;
using System;

public class Clickable
{
	public int ZOrder
	{
		get { return _zOrder; }
		set { _zOrder = value; }
	}

	public Clickable(
		InputManager inputMgr,
		IClickable entity,
		Action<uint> eventOnSelected = null,
		Action eventOnDeselected = null,
		Action<bool> eventOnTrySelected = null,
		Action<Point2D, Point2D> eventOnRelesed = null,
		int zOrder = 0)
	{
		_owner = entity;
		_eventOnSelected = eventOnSelected;
		_eventOnDeselected = eventOnDeselected;
		_eventOnTrySelected = eventOnTrySelected;
		_eventOnRelesed = eventOnRelesed;
		_zOrder = zOrder;

		inputMgr.AddClickable (_owner, zOrder);
	}

	public bool HandleTrySelect(Point2D pos)
	{
		if (_owner.Collide(pos))
		{
            if (_eventOnTrySelected != null)
            {
				_eventOnTrySelected (true);
                Color c = EngineLogic.Colors[_owner.PlayerId];
                c.a = c.a / 2;
                _owner.Color = c;
            }
			return true;
		}
        if (_eventOnTrySelected != null)
        {
			_eventOnTrySelected (false);
            _owner.Color = EngineLogic.Colors[_owner.PlayerId];
        }
		return false;
	}

	public void HandleSelected(uint playerId)
	{
        if (_eventOnSelected != null)
        {
            _eventOnSelected (playerId);
            Color c = EngineLogic.Colors[_owner.PlayerId];
            c.a = c.a / 2;
            _owner.Color = c;
        }
	}

	public void HandleDeselected()
	{
        if (_eventOnDeselected != null)
        {
            _eventOnDeselected ();
            _owner.Color = EngineLogic.Colors[_owner.PlayerId];
        }
	}

	// Call this when removing ..
	public void HandleRemove(InputManager inputMgr)
	{
		inputMgr.RemoveClickable(_owner);
	}


	private IClickable _owner;
	private Action<uint> _eventOnSelected;
	private Action _eventOnDeselected;
	private Action<bool> _eventOnTrySelected;
	private Action<Point2D, Point2D> _eventOnRelesed;
	private int _zOrder;
}
	