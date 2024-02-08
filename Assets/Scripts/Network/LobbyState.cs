using UnityEngine;
using System;
using System.Net;
using LitJson;


public class LobbyState : NetworkState
{
    public LobbyState(NetworkManager manager)
        :base(manager)
    {
		_handlePacket[typeof(StartPacket).ToString()] = HandleStartPacket;
		_handlePacket[typeof(ConnectedPacket).ToString()] = HandleConnectedPacket;
		_handlePacket[typeof(DisConnectedPacket).ToString()] = HandleDisConnectedPacket;
    }

    public override void UpdateAndSendByState()
    {
        // Let's process the events which are not handled.
		HandleNotHandledPackets();
    }
    
    private void HandleStartPacket(string data, ENet.Event @event)
    {
        StartPacket packet = JsonMapper.ToObject<StartPacket>(data);
        var fromAddr = @event.Peer.GetRemoteAddress();

        // Make sure this is the master peer, cause we don't want any funny business
        if (fromAddr.Equals(_owner.MasterPeer.GetRemoteAddress()))
        {
            uint seed = packet.Seed;
			_owner.Engine.SetStartingState (seed);

            // for now, assume that we're one frame off, but ideally we would RTT to adjust
            // the time to start, based on latency/jitter

            _owner.TimeToStart = NetworkManager.StartDelay; // - Timing::sInstance.GetDeltaTime();
            _owner.FSM.Fire(NetworkManager.Event.EnterStarting);
        }
    }


    private void HandleConnectedPacket(string data, ENet.Event @event)
    {
        Debug.Log("connect  id : " + @event.Data);

        ConnectedPacket packet = JsonMapper.ToObject<ConnectedPacket>(data);
        if (@event.Data != NetworkManager.Me)
        {
            if (_owner.IdToNameMap.Count >= NetworkManager.MaxPlayerCount)
            {
                Debug.Log("Sorry, we are full!");

                //sorry, can't join if full
                NotJoinablePacket notJoinablePacket = new NotJoinablePacket();
                @event.Peer.Send(NetworkManager.ChannelLimit, NetworkManager.CreatePacket(
                    JsonMapper.ToJson(notJoinablePacket)));
                return;
            }

            Debug.Log("Connected with : " + @event.Peer.GetRemoteAddress());
            YouConnectedMePacket youConnectMe = new YouConnectedMePacket(_owner.PlayerId);
            var enetPacket = NetworkManager.CreatePacket(JsonMapper.ToJson(youConnectMe));

            @event.Peer.Send(NetworkManager.ChannelLimit, enetPacket);
            _owner.FSM.Fire(NetworkManager.Event.ConnectionRequested);
			_owner.Engine.SetConnectingState ();
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
}
