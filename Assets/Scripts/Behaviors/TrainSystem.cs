using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainSystem 
{
	public Point2D Destination
	{
		get { return _destination; }
		set { _destination = value; }
	}

	public TrainCharacterCommand.TrainCharacterPacket TrainEntityName
	{
		get { return _trainEntityData; }
		set { _trainEntityData = value; }
	}

    public int TrainQCount
    {
        get { return _trainQ.Count; }
    }

    public TrainCharacterCommand.TrainCharacterPacket TrainQDequeue()
    {
        var data = _trainQ.Dequeue();
        
        return data;
    }

    public void TrainQEnqueue(TrainCharacterCommand.TrainCharacterPacket data)
    {
        ////////////////
        _trainQ.Enqueue(data);
    }

    //public Queue<TrainCharacterCommand.TrainCharacterPacket> TrainQ
    //{
    //	get { return _trainQ; }
    //}

    public TrainSystem(Entity entity, Point2D destination)
	{
		Debug.Assert (entity is ITrainable, "entity is not ITrainable!");

		_owner = entity;
		_destination = destination;
		_trainEntityData = null;
		_delayCnt = 0;
		_maxDelay = 0;

		_trainQ = new Queue<TrainCharacterCommand.TrainCharacterPacket> ();
	}

	public void SetNextTraining(TrainCharacterCommand.TrainCharacterPacket tdata)
	{
		var data = _owner.Engine.Database.CharacterDataEntry (tdata.CharacterType);
        _trainEntityData = tdata;
		_maxDelay = data.TrainSpeed;
	}

	public bool TrainDelayFinished()
	{
		_delayCnt++;
		if (_delayCnt % _maxDelay == 0) 
		{
			_delayCnt = 0;
			return true;
		}
		return false;
	}

	public virtual void TrainEntity()
	{
		_owner.Engine.World.CreateCharacter (
            _trainEntityData.CharacterType,
			_owner.PlayerId,
            _owner.Pos,
            _destination,
            _trainEntityData.RelatedEntities
            );
	}
		

	private Entity _owner;
	private Point2D _destination;
	private TrainCharacterCommand.TrainCharacterPacket _trainEntityData;
	private int _delayCnt;
	private int _maxDelay;
	private Queue<TrainCharacterCommand.TrainCharacterPacket> _trainQ;
}
