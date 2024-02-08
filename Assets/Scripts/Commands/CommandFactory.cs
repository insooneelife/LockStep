using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

public class CommandFactory
{
	public static CommandFactory Instance = null;

	public static void StaticInit()
	{
		Instance = new CommandFactory();
	}

	public Command Create(string data)
	{
		string type = Utils.FindProperty (data, "CommandType");

		Func<string, Command> createMethod = null;
		bool exists = _creator.TryGetValue (type, out createMethod);

		if (exists) 
		{
			return createMethod (data);
		}
		else 
		{
			Debug.Assert (false, "You must add create function into creator map in CommandFactory!");
			return null;
		}
	}

	private CommandFactory()
	{
		_creator = new SortedDictionary<string, Func<string, Command> > ();
        _creator ["CreateBuildingCommand"] = CreateBuildingCommand.Create;
        _creator ["CreateDestinationCommand"] = CreateDestinationCommand.Create;
		_creator ["TrainCharacterCommand"] = TrainCharacterCommand.Create;
		_creator ["MoveToCellCommand"] = MoveToCellCommand.Create;
        _creator ["GenCharacterCommand"] = GenCharacterCommand.Create;
    }

	private SortedDictionary<string, Func<string, Command> > _creator;
}
