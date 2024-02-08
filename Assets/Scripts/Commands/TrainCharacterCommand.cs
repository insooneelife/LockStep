using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;


// \"{\\\"CommandType\\\":\\\"TrainCharacterCommand\\\",\\\"PlayerId\\\":1,\\\"BuildingId\\\":8,\\\"CharacterType\\\":\\\"Builder\\\",\\\"RelatedEntities\\\":[{\\\"Key\\\":0,\\\"Value\\\":\\\"SoulStone\\\"}]}\"

public class TrainCharacterCommand : Command
{
    public static TrainCharacterCommand Create(string data)
    {
        return JsonMapper.ToObject<TrainCharacterCommand>(data);
    }

    public class TrainCharacterPacket : Command.CommandPacket
    {
        public override string CommandType
        {
            get { return "TrainCharacterCommand"; }
        }

        public TrainCharacterPacket()
        { }

        public uint BuildingId;
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

    public uint BuildingId
    {
        get { return _data.BuildingId; }
        set { _data.BuildingId = value; }
    }

    public string CharacterType
    {
        get { return _data.CharacterType; }
        set { _data.CharacterType = value; }
    }

    public List<KeyValuePair<int, string>> RelatedEntities
    {
        get { return _data.RelatedEntities; }
        set { _data.RelatedEntities = value; }
    }

    public TrainCharacterCommand()
    {
        _data = new TrainCharacterPacket();
    }

    public TrainCharacterCommand(uint playerId, uint buildingId, string characterType, List<KeyValuePair<int, string>> related)
    {
        _data = new TrainCharacterPacket();
        _data.PlayerId = playerId;
        _data.BuildingId = buildingId;
        _data.CharacterType = characterType;
        _data.RelatedEntities = related;
    }

    public override Command Clone()
    {
        return new TrainCharacterCommand(PlayerId, BuildingId, CharacterType, RelatedEntities);
    }

    public override void ProcessCommand(EngineLogic engine)
    {
        Entity trainer = engine.EntityMgr.GetEntity(_data.BuildingId);

        Debug.Assert(trainer != null, "trainer is null!!");
        Debug.Assert(trainer is ITrainable, "trainer is not ITrainable!!");

        if (PlayerId == engine.NetworkMgr.PlayerId)
        {
            Database.CharacterData data = engine.Database.CharacterDataEntry(_data.CharacterType);
            engine.UIMgr.SetGold(engine.UIMgr.Gold - data.NeedGold);
        }
        
        TrainSystem trainSys = (trainer as ITrainable).TrainSys;
        //trainSys.TrainQ.Enqueue(_data);
        trainSys.TrainQEnqueue(_data);
    }

    private TrainCharacterPacket _data;
}
