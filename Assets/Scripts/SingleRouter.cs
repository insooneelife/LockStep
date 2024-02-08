using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class ENetUDPRouter : PacketRouter
{
	public ENetUDPRouter(NetworkManager manager)
		:base(manager)
	{}

	public override void Send(string json)
	{
		var enetPacket = NetworkManager.CreatePacket(json);
		_owner.ServerHost.Broadcast(NetworkManager.ChannelLimit, ref enetPacket);
	}

	// Read stream from network and store them into queue.
	public override void Recv()
	{
		ReadFromHost(_owner.ServerHost);
		if(_owner.ClientHostActivated)
			ReadFromHost(_owner.ClientHost);
	}

	private void ReadFromHost(ENet.Host host)
	{
		ENet.Event @event;
		if (host.Service(0, out @event))
		{
			do
			{
				switch (@event.Type)
				{
				case ENet.EventType.Connect:
					_owner.PacketQ.Enqueue(new NetworkManager.ReceivedPacket(JsonMapper.ToJson((new ConnectedPacket())), @event));
					Debug.Log("Connect!");
					break;

				case ENet.EventType.Receive:
					string json = NetworkManager.StreamToString(@event.Packet.GetBytes());
					_owner.PacketQ.Enqueue(new NetworkManager.ReceivedPacket(json, @event));
					@event.Packet.Dispose();
					break;

				case ENet.EventType.Disconnect:
					_owner.PacketQ.Enqueue(new NetworkManager.ReceivedPacket(
						JsonMapper.ToJson((new DisConnectedPacket((uint)@event.Data))), @event));
					break;
				}
			}
			while (host.CheckEvents(out @event));
		}
	}
}
