using UnityEngine;
using System;
using LitJson;

public class HelloPeersState : NetworkState
{
    public HelloPeersState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(YouConnectedMePacket).ToString()] = HandleYouConnectedMePacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    
    public override void UpdateAndSendByState()
    {
        if (_owner.ConnectionBuffer.Count > 0)
        {
            var connection = _owner.ConnectionBuffer.Peek();
            _owner.ClientHost.Connect(connection.Ip, connection.Port, (int)_owner.PlayerId);
            _owner.FSM.Fire(NetworkManager.Event.ConnectionRemain);
        }
        else
        {
            // Say "I'm done!" to master.
            ImDoneMasterPacket imDone = new ImDoneMasterPacket();
            var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(imDone));
            _owner.MasterPeer.Send(NetworkManager.ChannelLimit, enetPacket);

            _owner.Briefing();
            _owner.FSM.Fire(NetworkManager.Event.Join);
			_owner.Engine.SetLobbyState ();
        }
    }

    private void HandleYouConnectedMePacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        YouConnectedMePacket packet = JsonMapper.ToObject<YouConnectedMePacket>(data);
        _owner.IdToClientHostConnectedPeerMap[packet.MyId] = @event.Peer;
    }

    private void HandleDisConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("disconnect  id : " + @event.Data);

        DisConnectedPacket packet = JsonMapper.ToObject<DisConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            Debug.Log("Save disconnection event!!");
			_owner.NotHandledPacketQ.Enqueue(new NetworkManager.ReceivedPacket(data, @event));
        }
        else
        {
            Debug.Log("This DisConnected packet didn't come from others!");
        }
    }
}


