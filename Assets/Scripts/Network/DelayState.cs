using UnityEngine;
using System;
using LitJson;

public class DelayState : NetworkState
{
    public DelayState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(TurnPacket).ToString()] = HandleTurnPacket;
	}

    public override void UpdateAndSendByState()
    {}
    
	private void HandleTurnPacket(string data, ENet.Event @event)
	{
		TurnPacket packet = JsonMapper.ToObject<TurnPacket>(data);
		IpPort ipPort = new IpPort(@event.Peer.GetRemoteAddress());

		_owner.HandleTurn (ipPort, packet.TurnNumber, packet.PlayerId, packet.Command);
		_owner.TryTurn();
	}
}
