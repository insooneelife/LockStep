using UnityEngine;
using System;
using System.Net;
using LitJson;

public class PlayingState : NetworkState
{
    public PlayingState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(TurnPacket).ToString()] = HandleTurnPacket;
    }

    public override void UpdateAndSendByState()
    {
		HandleNotHandledPackets ();

        if(_owner.Engine.World != null)
		    _owner.Engine.World.Update ();

		_owner.SubTurnNumber++;
		if(_owner.SubTurnNumber == NetworkManager.SubTurnsPerTurn)
		{
			_owner.SendTurnPackets();
			_owner.TryTurn();
		}
    }
		
    private void HandleTurnPacket(string data, ENet.Event @event)
    {
		TurnPacket packet = JsonMapper.ToObject<TurnPacket>(data);
		IpPort ipPort = new IpPort(@event.Peer.GetRemoteAddress());
		_owner.HandleTurn (ipPort, packet.TurnNumber, packet.PlayerId, packet.Command);
    }
}

