using UnityEngine;
using System;
using LitJson;

public class MasterAcceptConnectState : NetworkState
{
    public MasterAcceptConnectState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(HelloPacket).ToString()] = HandleHelloPacket;
		_handlePacket[typeof(YouConnectedMePacket).ToString()] = HandleYouConnectedMePacket;
		_handlePacket[typeof(ImDoneMasterPacket).ToString()] = HandleImDoneMasterPacket;
		_handlePacket[typeof(ConnectedPacket).ToString()] = HandleConnectedPacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    public override void UpdateAndSendByState()
    {}

    private void HandleHelloPacket(string data, ENet.Event @event)
    {
        HelloPacket packet = JsonMapper.ToObject<HelloPacket>(data);

		// If name collide ..
		bool found = false;
		foreach (var name in _owner.IdToNameMap) 
		{
			if (name.Value == packet.Name)
				found = true; 
		}

        // For now, if I already know of this player, throw away the packet
        // this doesn't work if there's packet loss
		if (_owner.ServerAddrToIdMap.ContainsKey(packet.IpPort) || found)
        {
            Debug.Log("This player already joined!");

            NotJoinablePacket notJoin = new NotJoinablePacket();
            var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(notJoin));
            @event.Peer.Send(NetworkManager.ChannelLimit, enetPacket);

            _owner.Briefing();
            _owner.FSM.Fire(NetworkManager.Event.ConnectionDone);
			_owner.Engine.SetLobbyState ();
            return;
        }
        
        // Welcome Packet ..
        string newPlayerName = packet.Name;
        uint newPlayerId = ++_owner.HighestPlayerId;
        WelcomePacket welcome =
            new WelcomePacket(
                newPlayerId,
                _owner.PlayerId,
                _owner.ServerAddrToIdMap,
                _owner.IdToNameMap);

        @event.Peer.Send(NetworkManager.ChannelLimit, NetworkManager.CreatePacket(JsonMapper.ToJson(welcome)));
        
        _owner.IdToServerHostConnectedPeerMap.Add(newPlayerId, @event.Peer);
        _owner.ServerAddrToIdMap.Add(packet.IpPort, newPlayerId);
        _owner.NameToServerAddrMap.Add(newPlayerName, packet.IpPort);
        _owner.IdToNameMap.Add(newPlayerId, newPlayerName);
        
        // Connect ..
        _owner.ClientHost.Connect(packet.IpPort.Ip, packet.IpPort.Port, (int)_owner.PlayerId);
        _owner.ClientHostActivated = true;
    }


    private void HandleYouConnectedMePacket(string data, ENet.Event @event)
    {
        YouConnectedMePacket packet = JsonMapper.ToObject<YouConnectedMePacket>(data);

        _owner.IdToClientHostConnectedPeerMap[packet.MyId] = @event.Peer;
        Debug.Log("Connection done for me(master peer)");
    }


    private void HandleImDoneMasterPacket(string data, ENet.Event @event)
    {
        Debug.Log("Connection finally done!");

        _owner.Briefing();
        _owner.FSM.Fire(NetworkManager.Event.ConnectionDone);
		_owner.Engine.SetLobbyState ();
    }



    private void HandleConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        ConnectedPacket packet = JsonMapper.ToObject<ConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            Debug.Log("Connection accured!! Try later! : " + @event.Peer.GetRemoteAddress());
            TryLaterPacket tryLater = new TryLaterPacket();
            var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(tryLater));
            @event.Peer.Send(NetworkManager.ChannelLimit, enetPacket);
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
