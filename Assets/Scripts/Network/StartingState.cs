using UnityEngine;
using System;
using LitJson;

public class StartingState : NetworkState
{
    public StartingState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket [typeof(ConnectedPacket).ToString ()] = HandleConnectedPacket;
		_handlePacket [typeof(DisConnectedPacket).ToString ()] = HandleDisConnectedPacket;
		_handlePacket [typeof(TurnPacket).ToString ()] = HandleTurnPacket;
    }
    
    public override void UpdateAndSendByState()
    {
        if(_owner.TimeToStart % 25 == 0)
            Debug.Log("Game start after : " + _owner.TimeToStart);

        _owner.TimeToStart -= 1;
        
        if (_owner.TimeToStart <= 0)
        {
            _owner.EnterPlayingState();
        }
    }

    private void HandleConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        ConnectedPacket packet = JsonMapper.ToObject<ConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            Debug.Log("Sorry, we are full! in starting");

            NotJoinablePacket notJoinablePacket = new NotJoinablePacket();
            @event.Peer.Send(NetworkManager.ChannelLimit, NetworkManager.CreatePacket(
                JsonMapper.ToJson(notJoinablePacket)));
        }
        else
        {
            Debug.Log("This Connected packet didn't come from others!");
        }
    }
    
    private void HandleDisConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("disconnect  id : " + @event.Data);

        DisConnectedPacket packet = JsonMapper.ToObject<DisConnectedPacket>(data);
        if (packet.PlayerId != NetworkManager.Me)
        {
            _owner.HandleConnectionReset(packet.PlayerId);
        }
        else
        {
            Debug.Log("This DisConnected packet didn't come from others!");
        }
    }

	private void HandleTurnPacket(string data, ENet.Event @event)
	{
		Debug.Log("Save turn event!!");
		_owner.NotHandledPacketQ.Enqueue(new NetworkManager.ReceivedPacket(data, @event));
	}
}

