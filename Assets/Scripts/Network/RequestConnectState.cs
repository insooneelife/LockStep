using UnityEngine;
using System;
using LitJson;

public class RequestConnectState : NetworkState
{
    public RequestConnectState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(YouConnectedMePacket).ToString()] = HandleYouConnectedMePacket;
		_handlePacket[typeof(ConnectedPacket).ToString()] = HandleConnectedPacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    public override void UpdateAndSendByState()
    {}
    

    private void HandleYouConnectedMePacket(string data, ENet.Event @event)
    {
        YouConnectedMePacket packet = JsonMapper.ToObject<YouConnectedMePacket>(data);
        _owner.IdToClientHostConnectedPeerMap[packet.MyId] = @event.Peer;

        // Intro
        IntroPacket intro = new IntroPacket(_owner.PlayerId, _owner.Name, _owner.ServerHostIpPort);
        var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(intro));
        @event.Peer.Send(NetworkManager.ChannelLimit, enetPacket);
    }

    private void HandleConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        ConnectedPacket packet = JsonMapper.ToObject<ConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            Debug.Log("Connected with : " + @event.Peer.GetRemoteAddress());

            _owner.IdToServerHostConnectedPeerMap.Add((uint)@event.Data, @event.Peer);

            YouConnectedMePacket youConnectMe = new YouConnectedMePacket(_owner.PlayerId);
            var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(youConnectMe));
            @event.Peer.Send(NetworkManager.ChannelLimit, enetPacket);

            _owner.Briefing();
            _owner.FSM.Fire(NetworkManager.Event.ConnectionDone);
            _owner.ConnectionBuffer.Dequeue();
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


