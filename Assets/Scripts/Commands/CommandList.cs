using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

public class CommandList
{
	public static CommandList Create(string data)
	{
		CommandList commands = new CommandList();
		string[] cmds = JsonMapper.ToObject<string[]> (data);
        Array.Sort(cmds, StringComparer.InvariantCulture);

        foreach (var cmd in cmds) 
		{
			commands.AddCommand (CommandFactory.Instance.Create(cmd));
		}
		return commands;
	}
		
	public CommandList()
	{
		_commands = new List<Command> ();
	}

	public CommandList Clone()
	{
		CommandList cmdlist = new CommandList ();
		foreach (var cmd in _commands) 
		{
			cmdlist.AddCommand (cmd.Clone());
		}
		return cmdlist;
	}
		
	public string ToStream()
	{
		JsonData array = JsonMapper.ToObject ("[]");
		foreach (var cmd in _commands) 
		{
			string json = JsonMapper.ToJson (cmd);
			array.Add (json);
		}
		return array.ToJson ();
	}

	public void Print()
	{
		foreach (var cmd in _commands) 
		{
			Debug.Log(JsonMapper.ToJson(cmd));
		}
	}

	public void AddCommand(Command command)
	{
		_commands.Add (command);
	}

	public void Clear()
	{
		_commands.Clear ();
	}

	public int Count()
	{
		return _commands.Count; 
	}

	public void	ProcessCommands(EngineLogic engine)
	{
		foreach (var cmd in _commands) 
		{
			cmd.Process (engine);
		}
	}
		
	private List<Command> _commands;
}