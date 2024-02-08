using UnityEngine;
using System;
using LitJson;

public class HelloState : NetworkState
{
    public HelloState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(NotMasterPeerPacket).ToString()] = HandleNotMasterPeerPacket;
		_handlePacket[typeof(WelcomePacket).ToString()] = HandleWelcomePacket;
		_handlePacket[typeof(YouConnectedMePacket).ToString()] = HandleYouConnectedMePacket;
		_handlePacket[typeof(TryLaterPacket).ToString()] = HandleTryLaterPacket;
		_handlePacket[typeof(NotJoinablePacket).ToString()] = HandleNotJoinablePacket;
		_handlePacket[typeof(ConnectedPacket).ToString()] = HandleConnectedPacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    public override void UpdateAndSendByState()
    {}


    private void UpdateSayingHello(bool inForce = false)
    {
        ulong time = _owner.Time;
        if (inForce || time > _owner.TimeOfLastHello + NetworkManager.TimeBetweenHellos)
        {
            HelloPacket helloPacket = new HelloPacket(_owner.Name, _owner.ServerHostIpPort);
            _owner.MasterPeer.Send(NetworkManager.ChannelLimit, NetworkManager.CreatePacket(JsonMapper.ToJson(helloPacket)));
            _owner.TimeOfLastHello = time;
        }
    }
    
    private void HandleNotMasterPeerPacket(string data, ENet.Event @event)
    {
        NotMasterPeerPacket packet = JsonMapper.ToObject<NotMasterPeerPacket>(data);

        @event.Peer.Disconnect((int)_owner.PlayerId);
        _owner.MasterPeer = _owner.ClientHost.Connect(packet.Ip, packet.Port, (int)_owner.PlayerId);
    }
    
    private void HandleWelcomePacket(string data, ENet.Event @event)
    {
        WelcomePacket packet = JsonMapper.ToObject<WelcomePacket>(data);

        // First is my player id
        _owner.UpdateHighestPlayerId(packet.NewPlayerId);
        _owner.PlayerId = packet.NewPlayerId;

        // Add me to the name map
        _owner.IdToNameMap.Add(packet.NewPlayerId, _owner.Name);

        // Now the player id for the master peer add entries for the master peer
        _owner.UpdateHighestPlayerId(packet.MasterPeerId);

        var masterPeerIpPort = new IpPort(_owner.MasterPeer.GetRemoteAddress());
        _owner.ServerAddrToIdMap.Add(masterPeerIpPort, packet.MasterPeerId);

        // Now remaining players
        var idToNameMap = packet.IdToNameMap();
        foreach (var e in idToNameMap)
        {
            _owner.IdToNameMap.Add(e.Key, e.Value);
        }

        var addrToIdMap = packet.AddrToIdMap();
        foreach (var e in addrToIdMap)
        {
            IpPort ipPort = e.Key;
            uint id = e.Value;
            _owner.ServerAddrToIdMap.Add(ipPort, id);
            _owner.NameToServerAddrMap.Add(_owner.IdToNameMap[id], ipPort);

            // For connect peers in next states.
            _owner.ConnectionBuffer.Enqueue(ipPort);
        }

        _owner.NameToServerAddrMap.Add(_owner.IdToNameMap[_owner.ServerAddrToIdMap[masterPeerIpPort]], masterPeerIpPort);
    }

    private void HandleYouConnectedMePacket(string data, ENet.Event @event)
    {
        YouConnectedMePacket packet = JsonMapper.ToObject<YouConnectedMePacket>(data);

        _owner.IdToClientHostConnectedPeerMap[packet.MyId] = @event.Peer;
        UpdateSayingHello(true);
    }

    private void HandleTryLaterPacket(string data, ENet.Event @event)
    {
        Debug.Log("Master is busy for connecting other peer, let's try later!");
    }

    private void HandleNotJoinablePacket(string data, ENet.Event @event)
    {
        Debug.Log("Not joinable, exiting...");
        @event.Peer.Disconnect((int)_owner.PlayerId);
        // exit
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

            _owner.FSM.Fire(NetworkManager.Event.IntroduceToPeers);
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


