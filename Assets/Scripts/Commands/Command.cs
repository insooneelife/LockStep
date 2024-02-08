using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Command 
{
	public abstract class CommandPacket
	{
		public abstract string CommandType { get ; }
		public uint PlayerId;
	}

	// This one must defined first(child classes too), so that JsonReader can fastly find what type the stream is.
	public abstract string CommandType { get; }

	public void Process(EngineLogic engine)
	{
		uint rand = engine.RandGen (0, 2000000000);
		//Debug.Log("[" + ToString() + "] processed in Turn : " + engine.NetworkMgr.TurnNumber + "  Rand : " + rand);
		ProcessCommand (engine);
	}

	public abstract Command Clone ();
	public abstract void ProcessCommand (EngineLogic engine);
}