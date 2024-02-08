using UnityEngine;
using System.Collections.Generic;

public class Message
{
	public enum MsgType
	{
		None, Damage
	}

	// The entity that sent this telegram
	public uint Sender
	{
		get { return _sender; }
	}

	public uint Receiver
	{
		get { return _receiver; }
	}

	public MsgType Msg
	{
		get { return _msg; }
	}

	public System.Object ExtraInfo
	{
		get { return _extraInfo; }
	}

	public Message()
	{
		_sender = 0;
		_receiver = 0;
		_msg = MsgType.None;
		_extraInfo = null;
	}

	public Message(
		uint sender,
		uint receiver,
		MsgType msg,
		System.Object extraInfo = null)
	{
		_sender = sender;
		_receiver = receiver;
		_msg = msg;
		_extraInfo = extraInfo;
	}

	// The entity that sent this telegram
	uint _sender;

	// The entity that is to receive this telegram
	uint _receiver;

	// The message itself. These are all enumerated in the file
	MsgType _msg;

	// Any additional information that may accompany the message
	System.Object _extraInfo;
};

public class EntityManager
{
	public const uint InvalidateId = 0;

	public EntityManager(EngineLogic engine)
	{
		_engine = engine;
		_nextGenId = InvalidateId;
		_entities = new SortedDictionary<uint, Entity> ();
	}

	public uint IDGen()
	{
		return ++_nextGenId;
	}

	public Entity GetEntity(uint id)
	{
		Entity entity = null;
		bool exsists = _entities.TryGetValue (id, out entity);

		if (exsists)
			return entity;
		
		return null;
	}

	public bool Exists(uint id)
	{
		return _entities.ContainsKey (id);
	}

	public void RegisterEntity(Entity entity)
	{
		_entities [entity.Id] = entity;
	}

	public void UnRegisterEntity(Entity entity)
	{
		_entities.Remove (entity.Id);
	}

	public void DispatchMsg(
		uint senderId,
		uint receiverId,
		Message.MsgType msgType,
		System.Object extraInfo)
	{
		// Get a pointer to the receiver
		Entity receiver = GetEntity(receiverId);

		// Make sure the receiver is valid
		if (receiver == null)
			return;
	
		// Create the telegram
		Message msg = new Message(senderId, receiverId, msgType, extraInfo);

		// Send the telegram to the recipient
		Discharge(receiver, msg);
	}

	// This method is utilized by DispatchMsg or DispatchDelayedMessages.
	// This method calls the message handling member function of the receiving
	// entity, pReceiver, with the newly created telegram
	void Discharge(IMessageHandler receiver, Message msg)
	{
		if (receiver == null)
			return;

		if (!receiver.HandleMessage(msg))
		{}
	}

	private EngineLogic _engine;
	private uint _nextGenId;
	private SortedDictionary< uint, Entity > _entities;
}
	