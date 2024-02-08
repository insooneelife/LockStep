using UnityEngine;
using System.Collections;
using LitJson;

// "{ \"CommandType\" : \"CreateBuildingCommand\", \"PlayerId\" : 1, \"CreateName\" : \"SoulStone\", \"Pos\" : { \"x\" :-1200, \"y\" : -600 } }";

public class CreateBuildingCommand : Command
{
	public static CreateBuildingCommand Create(string data)
	{
		return JsonMapper.ToObject<CreateBuildingCommand> (data);
	}

	public class CreateBuildingPacket : Command.CommandPacket
	{
		public override string CommandType
		{
			get { return "CreateBuildingCommand"; }
		}

		public CreateBuildingPacket()
		{}

		public string CreateName;
		public Point2D Pos;
	}

	public override string CommandType
	{
		get { return _data.CommandType; }
	}

	public uint PlayerId
	{
		get{ return _data.PlayerId; }
		set { _data.PlayerId = value; }
	}

	public string CreateName
	{
		get{ return _data.CreateName; }
		set { _data.CreateName = value; }
	}

	public Point2D Pos
	{
		get{ return _data.Pos; }
		set { _data.Pos = value; }
	}

	public CreateBuildingCommand()
	{
		_data = new CreateBuildingPacket ();
	}

	public CreateBuildingCommand(uint playerId, string createName, Point2D localPos) 
	{
		_data = new CreateBuildingPacket ();
		_data.PlayerId = playerId;
		_data.CreateName = createName;
		_data.Pos = localPos;
	}

	public override Command Clone()
	{
		return new CreateBuildingCommand (PlayerId, CreateName, Pos);
	}

	public override void ProcessCommand(EngineLogic engine)
	{
		engine.World.CreateBuilding (_data.CreateName, _data.PlayerId, _data.Pos);
	}
		
	private CreateBuildingPacket _data;
}
