using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    Thread networkThread;
    Thread mainThread;

    string myName;
    Color myColor;

    private bool stopConnection = false;
    public EventBasedNetListener networkListener;
    public NetManager client;

    public bool isHost;

    private string host = "localhost";
    private int port;
    private string password;
    private int maxPlayers;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();
    }

    public void InitGame(string hostaddress, int port, string pwd, int maxPlayers, bool isHost)
    {
        stopConnection = false;
        this.isHost = isHost;
        this.maxPlayers = maxPlayers;
        this.password = pwd;
        this.host = hostaddress == "" ? "localhost" : hostaddress;
        this.port = port;
        
        myName = "test " + Random.value;
        myColor = new Color(Random.value, Random.value, Random.value);
        print($"Initgame: Name:{myName} color:{myColor.ToString()}");
        mainThread = Thread.CurrentThread;
        networkThread = new Thread(UpdateNetwork);
        networkThread.Start();
    }

    public void UpdateNetwork()
    {
        networkListener = new EventBasedNetListener();

        networkListener.NetworkReceiveEvent += ReceivePackage;
        networkListener.PeerDisconnectedEvent += (peer, info) =>
        {
            Debug.Log(info.Reason.ToString());
            stopConnection = true;
        };

        client = new NetManager(networkListener);
        if (isHost)
            client.Start(port);
        else
            client.Start();

        if (isHost)
        {
            print($"Starting as host {host}:{port} with pwd {password}");
            networkListener.ConnectionRequestEvent += OnConnectionRequestEvent;
            networkListener.PeerConnectedEvent += OnPeerConnectedEvent;
        }
        else
        {
            print($"Trying to connect to: {host}:{port} with pwd {password}");
            client.Connect(host, port, password);
        }

        NetDataWriter writer = new NetDataWriter();
        while (!stopConnection)
        {
            client.PollEvents();
            Thread.Sleep(16);
            print("polling :" + client.ConnectedPeerList.Count);
        }
        client.Stop();
        print("End of updatenetwork");
    }

    void ReceivePackage(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        NetDataWriter writer = new NetDataWriter();
        ushort msgid = reader.GetUShort();
        switch (msgid)
        {
            case 1:

                break;
        }
    }

    void InitPlayer(string name, Color color)
    {

    }

    public void OnConnectionRequestEvent(ConnectionRequest request)
    {
        print("Received connection request");
        if (client.PeersCount < maxPlayers)
            request.AcceptIfKey(password);
        else
            request.Reject();
    }

    public void OnPeerConnectedEvent(NetPeer peer)
    {
        print("connection from: " + peer.EndPoint);
        NetDataWriter writer = new NetDataWriter();
        writer.Put("test");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void OnApplicationQuit()
    {
        client.Stop();
    }

}
