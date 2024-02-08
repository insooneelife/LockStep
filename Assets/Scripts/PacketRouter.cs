using UnityEngine;
using System.Collections.Generic;

public abstract class PacketRouter 
{
	public PacketRouter(NetworkManager manager)
	{
		_owner = manager;
	}

	public abstract void Send(string packet);
	public abstract void Recv();

	protected NetworkManager _owner;
}
