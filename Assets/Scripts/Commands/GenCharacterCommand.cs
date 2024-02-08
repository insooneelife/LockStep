using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class GenCharacterCommand : Command
{
	public static GenCharacterCommand Create(string data)
	{
		return JsonMapper.ToObject<GenCharacterCommand> (data);
	}

	public class GenCharacterPacket : Command.CommandPacket
	{
		public override string CommandType
		{
			get { return "GenCharacterCommand"; }
		}

		public GenCharacterPacket()
		{}

		public Point2D Pos;
		public string CharacterType;
        public List<KeyValuePair<int, string>> RelatedEntities;
    }
		
	public override string CommandType
	{
		get { return _data.CommandType; }
	}

	public uint PlayerId
	{
		get { return _data.PlayerId; }
		set { _data.PlayerId = value; }
	}

	public Point2D Pos
	{
		get{ return _data.Pos; }
		set { _data.Pos = value; }
	}

	public string CharacterType
	{
		get{ return _data.CharacterType; }
		set { _data.CharacterType = value; }
	}

    public List<KeyValuePair<int, string>> RelatedEntities
    {
        get { return _data.RelatedEntities; }
        set { _data.RelatedEntities = value; }
    }

    public GenCharacterCommand()
	{
		_data = new GenCharacterPacket ();
	}

	public GenCharacterCommand(uint playerId, Point2D pos, string characterType, List<KeyValuePair<int, string>> related) 
	{
		_data = new GenCharacterPacket();
		_data.PlayerId = playerId;
		_data.Pos = pos;
		_data.CharacterType = characterType;
        _data.RelatedEntities = related;

    }

	public override Command Clone()
	{
		return new GenCharacterCommand (PlayerId, Pos, CharacterType, RelatedEntities);
	}

	public override void ProcessCommand(EngineLogic engine)
	{
        engine.World.CreateCharacter(
            _data.CharacterType,
            _data.PlayerId,
            _data.Pos,
            _data.Pos, 
            _data.RelatedEntities);
    }
		
	private GenCharacterPacket _data;
}
