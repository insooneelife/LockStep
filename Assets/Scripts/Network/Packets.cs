using LitJson;
using System;
using System.Net;
using System.Collections.Generic;

public class Packet
{
	public string Type
	{
		get { return ToString(); }
	}
}

public class ConnectedPacket : Packet
{
	public ConnectedPacket()
	{ }
}

public class DisConnectedPacket : Packet
{
	public DisConnectedPacket()
	{ }

	public DisConnectedPacket(uint playerId)
	{
		PlayerId = playerId;
	}

	public uint PlayerId;
}

public class YouConnectedMePacket : Packet
{
	public YouConnectedMePacket()
	{ }

	public YouConnectedMePacket(uint myId)
	{
		MyId = myId;
	}

	public uint MyId;
}


public class HelloPacket : Packet
{
	public HelloPacket()
	{ }

	public HelloPacket(string name, IpPort ipPort)
	{
		Name = name;
		IpPort = ipPort;
	}

	public string Name;
	public IpPort IpPort;
}

public class NotJoinablePacket : Packet
{
	public NotJoinablePacket()
	{ }
}

public class NotMasterPeerPacket : Packet
{
	public string Ip
	{
		get { return Address.Ip; }
	}

	public int Port
	{
		get { return Address.Port; }
	}

	public IPEndPoint EndPoint()
	{
		return Address.EndPoint();
	}

	public NotMasterPeerPacket()
	{ }

	public NotMasterPeerPacket(IpPort addr)
	{
		Address = addr;
	}

	public IpPort Address;
}


// Master peer sends this packet when a new peer sent HelloPacket to him.
public class WelcomePacket : Packet
{
	public SortedDictionary<uint, string> IdToNameMap()
	{
		var adaptor = new UIntToStringMapAdaptor();
		adaptor.MapStream = IdToNameMapStream;
		return adaptor.Map();   
	}

	public SortedDictionary<IpPort, uint> AddrToIdMap()
	{
		var adaptor = new IpPortToUIntMapAdaptor();
		adaptor.MapStream = AddrToIdMapStream;
		return adaptor.Map();
	}

	public WelcomePacket()
	{ }

	public WelcomePacket(
		uint newPlayerId,
		uint masterPeerId,
        SortedDictionary<IpPort, uint> addrToIdMap,
        SortedDictionary<uint, string> idToNameMap)
	{
		NewPlayerId = newPlayerId;
		MasterPeerId = masterPeerId;

		IdToNameMapStream = (new UIntToStringMapAdaptor(idToNameMap)).MapStream;
		AddrToIdMapStream = (new IpPortToUIntMapAdaptor(addrToIdMap)).MapStream;
	}

	public uint NewPlayerId;
	public uint MasterPeerId;
	public string IdToNameMapStream;
	public string AddrToIdMapStream;
}


public class IntroPacket : Packet
{
	public IntroPacket()
	{ }

	public IntroPacket(uint newPlayerId, string newPlayerName, IpPort ipPort)
	{
		NewPlayerId = newPlayerId;
		NewPlayerName = newPlayerName;
		IpPort = ipPort;
	}

	public uint NewPlayerId;
	public string NewPlayerName;
	public IpPort IpPort;
}

public class TryLaterPacket : Packet
{
	public TryLaterPacket()
	{ }
}

public class ImDoneMasterPacket : Packet
{
	public ImDoneMasterPacket()
	{ }
}


public class StartPacket : Packet
{
	public StartPacket()
	{ }

	public StartPacket(uint seed)
	{
		Seed = seed;
	}

	public uint Seed;
}

public class TurnPacket : Packet
{
	public TurnPacket()
	{ }

	public TurnPacket(int turnNum, uint playerId, string command)
	{
		TurnNumber = turnNum;
		PlayerId = playerId;
		Command = command;
	}

	public int TurnNumber;
	public uint PlayerId;
	public string Command;
}













// For compatibility
public class IpPort
{
    public class Comparer : IComparer<IpPort>
    {
        public int Compare(IpPort x, IpPort y)
        {
            return x.ToString().CompareTo(y.ToString());
        }
    }

    public IPEndPoint EndPoint()
	{
		IPAddress addr = IPAddress.Parse(Ip);
		IPEndPoint endPoint = new IPEndPoint(addr, Port);
		return endPoint;
	}

	public string Ip
	{
		get
		{
			char[] separators = { ':' };
			string[] words = Addr.Split(separators);

			return words[0];
		}
	}

	public int Port
	{
		get
		{
			char[] separators = { ':' };
			string[] words = Addr.Split(separators);

			return Int32.Parse(words[1]);
		}
	}

	public IpPort() { }

	public IpPort(IPEndPoint addr)
	{
		Addr = addr.ToString();
	}

	public IpPort(string ip, int port)
	{
		Addr = ip + ":" + port.ToString();
	}

	public IpPort(string addr)
	{
		Addr = addr;
	}

	public override string ToString()
	{
		return Addr;
	}

	public override int GetHashCode()
	{
		return Addr.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return this.Addr.Equals(((IpPort)obj).Addr);
	}

	public string Addr;
}



public class IpPortToUIntMapAdaptor
{
	public SortedDictionary<IpPort, uint> Map()
	{
		JsonReader reader = new JsonReader(MapStream);
        SortedDictionary<IpPort, uint> newMap = new SortedDictionary<IpPort, uint>(new IpPort.Comparer());
		IpPort key = null;
		uint value;
		while (reader.Read())
		{
			if (reader.Token == JsonToken.ObjectStart)
				continue;

			else if (reader.Token == JsonToken.ObjectEnd)
				break;

			else if (reader.Token == JsonToken.PropertyName)
			{
				key = new IpPort((string)reader.Value);
			}
			else
			{
				value = (uint)(int)reader.Value;
				newMap[key] = value;
			}
		}
		return newMap;
	}

	public IpPortToUIntMapAdaptor()
	{ }

	public IpPortToUIntMapAdaptor(SortedDictionary<IpPort, uint> map)
	{
		if (map.Count > 0)
		{
			JsonData data = new JsonData();

			foreach (var e in map)
			{
				data[e.Key.ToString()] = e.Value;
			}

			MapStream = data.ToJson();
		}
		else
		{
			MapStream = "{}";
		}
	}

	public override string ToString()
	{
		return MapStream;
	}

	public string MapStream;
}


public class UIntToStringMapAdaptor
{
	public SortedDictionary<uint, string> Map()
	{
		JsonReader reader = new JsonReader(MapStream);
        SortedDictionary<uint, string> newMap = new SortedDictionary<uint, string>();
		uint key = 0;
		string value = "";
		while (reader.Read())
		{
			if (reader.Token == JsonToken.ObjectStart)
				continue;

			else if (reader.Token == JsonToken.ObjectEnd)
				break;

			else if (reader.Token == JsonToken.PropertyName)
			{
				key = UInt32.Parse((string)reader.Value);
			}
			else
			{
				value = (string)reader.Value;
				newMap[key] = value;
			}
		}
		return newMap;
	}

	public UIntToStringMapAdaptor()
	{ }

	public UIntToStringMapAdaptor(SortedDictionary<uint, string> map)
	{
		if (map.Count > 0)
		{
			JsonData data = new JsonData();

			foreach (var e in map)
			{
				data[e.Key.ToString()] = e.Value;
			}

			MapStream = data.ToJson();
		}
		else
		{
			MapStream = "{}";
		}
	}

	public override string ToString()
	{
		return MapStream;
	}

	public string MapStream;
}