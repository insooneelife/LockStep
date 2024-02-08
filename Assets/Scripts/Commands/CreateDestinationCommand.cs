using UnityEngine;
using System.Collections;
using LitJson;


// \"{\\\"CommandType\\\":\\\"CreateDestinationCommand\\\",\\\"PlayerId\\\":1,\\\"FromBuildingId\\\":9,\\\"Destination\\\":{\\\"x\\\":119,\\\"y\\\":887}}\"

public class CreateDestinationCommand : Command
{
	public static CreateDestinationCommand Create(string data)
	{
		return JsonMapper.ToObject<CreateDestinationCommand> (data);
	}

	public class CreateDestinationPacket : Command.CommandPacket
	{
		public override string CommandType
		{
			get { return "CreateDestinationCommand"; }
		}

		public CreateDestinationPacket()
		{}
		public uint FromBuildingId;
		public Point2D Destination;
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

	public uint FromBuildingId
	{
		get{ return _data.FromBuildingId; }
		set { _data.FromBuildingId = value; }
	}

	public Point2D Destination
	{
		get{ return _data.Destination; }
		set { _data.Destination = value; }
	}

	public CreateDestinationCommand()
	{
		_data = new CreateDestinationPacket ();
	}

	public CreateDestinationCommand(uint playerId, uint fromBuildingId, Point2D destination) 
	{
		_data = new CreateDestinationPacket ();
		_data.PlayerId = playerId;
		_data.FromBuildingId = fromBuildingId;
		_data.Destination = destination;
	}

	public override Command Clone()
	{
		return new CreateDestinationCommand (PlayerId, FromBuildingId, Destination);
	}

	public override void ProcessCommand(EngineLogic engine)
	{
		Entity fromBuilding = engine.EntityMgr.GetEntity (_data.FromBuildingId);
        if (fromBuilding == null)
            return;

		TrainSystem trainSys = (fromBuilding as ITrainable).TrainSys;
        trainSys.Destination = _data.Destination;
    }
		
	private CreateDestinationPacket _data;
}
