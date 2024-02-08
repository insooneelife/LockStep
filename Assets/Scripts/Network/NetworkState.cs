using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;

public abstract class NetworkState
{
    public class Comparer : IComparer<NetworkState>
    {
        public int Compare(NetworkState x, NetworkState y)
        {
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }

    public NetworkState(NetworkManager mgr)
    {
        _owner = mgr;
		_handlePacket = new SortedDictionary<string, Action<string, ENet.Event>>();
    }

    public void HandlePackets(string data, ENet.Event @event)
    {
		string type = Utils.FindProperty (data, "Type");

        //Debug.Log("packet : [ " + type + " ] handled in state : \""
        //    + _owner.FSM.State.ToString() + "\"");
        //Debug.Log(data);

        Action<string, ENet.Event> actionHandle = null;
        bool exists = _handlePacket.TryGetValue(type, out actionHandle);

        if (exists)
        {
            actionHandle(data, @event);
        }
        else
        {
            Debug.Log("Unexpected packet received [ "
              + type + " ] in [ "
              + _owner.FSM.State.ToString() + " ] state. Ignoring.");
        }
    }

	public void UpdateAndSend()
    {
        UpdateAndSendByState();
    }

	protected void HandleNotHandledPackets()
	{
		while (_owner.NotHandledPacketQ.Count > 0) 
		{
			_owner.PacketQ.Enqueue (_owner.NotHandledPacketQ.Dequeue ());
		}
	}

    public abstract void UpdateAndSendByState();
    
    protected NetworkManager _owner;

    // < packetType, Action<data, @event> >
	protected SortedDictionary<string, Action<string, ENet.Event>> _handlePacket;
   
}
