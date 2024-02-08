using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class LanManager {
	// Addresses of the computer (Ethernet, WiFi, etc.)
	public List<string> _localAddresses { get; private set; }
	public List<string> _localSubAddresses { get; private set; }

	// Addresses found on the network with a server launched
	public List<string> _addresses { get; private set; }

	public bool _isSearching { get; private set; }
	public float _percentSearching { get; private set; }

	private Socket _socketServer;
	private Socket _socketClient;

	private EndPoint _remoteEndPoint;

	public LanManager() {
		_addresses = new List<string>();
		_localAddresses = new List<string>();
		_localSubAddresses = new List<string>();
	}

	public void StartServer(int port) {
		if (_socketServer == null && _socketClient == null) {
			try {
				_socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				if (_socketServer == null) {
					Debug.LogWarning("SocketServer creation failed");
					return;
				}

				// Check if we received pings
				_socketServer.Bind(new IPEndPoint(IPAddress.Any, port));

				_remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

				_socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
					ref _remoteEndPoint, new AsyncCallback(_ReceiveServer), null);
			} catch (Exception ex) {
				Debug.Log(ex.Message);
			}
		}
	}

	public void CloseServer() {
		if (_socketServer != null) {
			_socketServer.Close();
			_socketServer = null;
		}
	}

	public void StartClient(int port) {
		if (_socketServer == null && _socketClient == null) {
			try {
				_socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				if (_socketClient == null) {
					Debug.LogWarning("SocketClient creation failed");
					return;
				}

				// Check if we received response from a remote (server)
				_socketClient.Bind(new IPEndPoint(IPAddress.Any, port));

				_socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				_socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

				_remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

				_socketClient.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
					ref _remoteEndPoint, new AsyncCallback(_ReceiveClient), null);
			} catch (Exception ex) {
				Debug.Log(ex.Message);
			}
		}
	}

	public void CloseClient() {
		if (_socketClient != null) {
			_socketClient.Close();
			_socketClient = null;
		}
	}

	public IEnumerator SendPing(int port) {
		_addresses.Clear();

		if (_socketClient != null) {
			int maxSend = 4;
			float countMax = (maxSend * _localSubAddresses.Count) - 1;

			float index = 0;

			_isSearching = true;

			// Send several pings just to be sure (a ping can be lost!)
			for (int i = 0 ; i < maxSend ; i++) {

				// For each address that this device has
				foreach (string subAddress in _localSubAddresses) {
					IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(subAddress + ".255"), port);
					byte[] str = Encoding.ASCII.GetBytes("ping");

					_socketClient.SendTo(str, destinationEndPoint);

					_percentSearching = index / countMax;

					index++;

					yield return new WaitForSeconds(0.1f);
				}
			}
			_isSearching = false;
		}
	}

	private void _ReceiveServer(IAsyncResult ar) {
		if (_socketServer != null) {
			try {
				int size = _socketServer.EndReceiveFrom(ar, ref _remoteEndPoint);
				byte[] str = Encoding.ASCII.GetBytes("pong");

				// Send a pong to the remote (client)
				_socketServer.SendTo(str, _remoteEndPoint);

				_socketServer.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
					ref _remoteEndPoint, new AsyncCallback(_ReceiveServer), null);
			} catch (Exception ex) {
				Debug.Log(ex.ToString());
			}
		}
	}

	private void _ReceiveClient(IAsyncResult ar) {
		if (_socketClient != null) {
			try {
				int size = _socketClient.EndReceiveFrom(ar, ref _remoteEndPoint);
				string address = _remoteEndPoint.ToString().Split(':')[0];

				// This is no ourself and we do not already have this address
				if (!_localAddresses.Contains(address) && !_addresses.Contains(address)) {
					_addresses.Add(address);
				}

				_socketClient.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None,
					ref _remoteEndPoint, new AsyncCallback(_ReceiveClient), null);
			} catch (Exception ex) {
				Debug.Log(ex.ToString());
			}
		}
	}

	public void ScanHost() {
		IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

		foreach (IPAddress ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				string address = ip.ToString();
				string subAddress = address.Remove(address.LastIndexOf('.'));

				_localAddresses.Add(address);

				if (!_localSubAddresses.Contains(subAddress)) {
					_localSubAddresses.Add(subAddress);
				}
			}
		}
	}
}