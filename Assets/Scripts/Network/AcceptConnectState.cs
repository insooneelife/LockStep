using UnityEngine;
using System;
using LitJson;

public class AcceptConnectState : NetworkState
{
    public AcceptConnectState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(IntroPacket).ToString()] = HandleIntroPacket;
		_handlePacket[typeof(YouConnectedMePacket).ToString()] = HandleYouConnectedMePacket;
		_handlePacket[typeof(HelloPacket).ToString()] = HandleHelloPacket;
		_handlePacket[typeof(ConnectedPacket).ToString()] = HandleConnectedPacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    public override void UpdateAndSendByState()
    {}

    private void HandleIntroPacket(string data, ENet.Event @event)
    {
        IntroPacket packet = JsonMapper.ToObject<IntroPacket>(data);

        uint newPlayerId = packet.NewPlayerId;
        string newPlayerName = packet.NewPlayerName;
        IpPort ipPort = packet.IpPort;


		Debug.Log ("Packets ip : " + ipPort.Addr);
		Debug.Log ("[In map]");
		foreach(var ip in  _owner.ServerAddrToIdMap)
		{
			Debug.Log("Key : " + ip.Key.Addr + "   Equals : " + ipPort.Equals(ip.Key));
		}


        _owner.UpdateHighestPlayerId(newPlayerId);
        _owner.IdToServerHostConnectedPeerMap.Add(newPlayerId, @event.Peer);
        _owner.ServerAddrToIdMap.Add(ipPort, newPlayerId);
        _owner.NameToServerAddrMap.Add(newPlayerName, ipPort);
        _owner.IdToNameMap.Add(newPlayerId, newPlayerName);

        // Connect ..
        _owner.ClientHost.Connect(ipPort.Ip, ipPort.Port, (int)_owner.PlayerId);
    }

    private void HandleYouConnectedMePacket(string data, ENet.Event @event)
    {
        YouConnectedMePacket packet = JsonMapper.ToObject<YouConnectedMePacket>(data);
        
        _owner.IdToClientHostConnectedPeerMap[packet.MyId] = @event.Peer;
        _owner.Briefing();
        _owner.FSM.Fire(NetworkManager.Event.ConnectionDone);
		_owner.Engine.SetLobbyState ();

        Debug.Log("Connection Done!");
    }

    private void HandleHelloPacket(string data, ENet.Event @event)
    {
        HelloPacket packet = JsonMapper.ToObject<HelloPacket>(data);

        // Talk to the master peer, not me ..
        NotMasterPeerPacket notMaster =
            new NotMasterPeerPacket(new IpPort(_owner.MasterPeer.GetRemoteAddress()));
        Debug.Log("Master peer address : " + _owner.MasterPeer.GetRemoteAddress().ToString());

        _owner.Briefing();
        _owner.FSM.Fire(NetworkManager.Event.ConnectionDone);
		_owner.Engine.SetLobbyState ();
        @event.Peer.Send(NetworkManager.ChannelLimit, NetworkManager.CreatePacket(JsonMapper.ToJson(notMaster)));
    }

    private void HandleConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        ConnectedPacket packet = JsonMapper.ToObject<ConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            Debug.Log("Save connection event!! : " + @event.Peer.GetRemoteAddress());
			_owner.NotHandledPacketQ.Enqueue(new NetworkManager.ReceivedPacket(data, @event));
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
