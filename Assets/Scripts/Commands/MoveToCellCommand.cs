using UnityEngine;
using System.Collections.Generic;
using LitJson;

// \"{\\\"CommandType\\\":\\\"MoveToCellCommand\\\",\\\"PlayerId\\\":1,\\\"From\\\":{\\\"x\\\":-88,\\\"y\\\":801},\\\"To\\\":{\\\"x\\\":197,\\\"y\\\":1064},\\\"IdList\\\":[]}\"

public class MoveToCellCommand : Command
{
	public static MoveToCellCommand Create(string data)
	{
		return JsonMapper.ToObject<MoveToCellCommand> (data);
	}

	public class MoveToCellPacket : Command.CommandPacket
	{
		public override string CommandType
		{
			get { return "MoveToCellCommand"; }
		}

		public MoveToCellPacket()
		{}
		public Point2D From;
		public Point2D To;
		public List<uint> IdList;
        public bool Important;
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

	public Point2D From
	{
		get{ return _data.From; }
		set { _data.From = value; }
	}

	public Point2D To
	{
		get{ return _data.To; }
		set { _data.To = value; }
	}

	public List<uint> IdList
	{
		get { return _data.IdList; }
		set { _data.IdList = value; }
	}

    public bool Important
    {
        get { return _data.Important; }
        set { _data.Important = value; }
    }

    public MoveToCellCommand()
	{
		_data = new MoveToCellPacket ();
	}

	public MoveToCellCommand(uint playerId, Point2D from, Point2D to, List<uint> idList, bool important) 
	{
		_data = new MoveToCellPacket ();
		_data.PlayerId = playerId;
		_data.From = from;
		_data.To = to;
		_data.IdList = idList;
        _data.Important = important;
    }

	public MoveToCellCommand(MoveToCellCommand copy)
	{
		_data = new MoveToCellPacket ();
		_data.PlayerId = copy.PlayerId;
		_data.From = copy.From;
		_data.To = copy.To;

		_data.IdList = new List<uint> ();
		foreach (var id in copy.IdList) 
		{
			_data.IdList.Add (id);
		}
        _data.Important = copy.Important;

    }

	public override Command Clone()
	{
		return new MoveToCellCommand (this);
	}

	public override void ProcessCommand(EngineLogic engine)
	{
		foreach (var id in _data.IdList) 
		{
			Entity ent = engine.EntityMgr.GetEntity (id);
			if (ent != null) 
			{
                Point2D wFrom = _data.From;
                Point2D wTo = _data.To;

                IMovable movingEnt = ent as IMovable;
				movingEnt.Move.HasDestination = true;
				movingEnt.Move.Destination = movingEnt.Pos + wTo - wFrom;
                movingEnt.Move.Important = _data.Important;
                ent.FSM.Fire(Entity.Event.HasDestination);
            }
		}
	}

	private MoveToCellPacket _data;
}