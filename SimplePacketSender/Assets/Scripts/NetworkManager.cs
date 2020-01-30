using Assets.Scripts.Extensions;
using Assets.Scripts.Packets.GamePackets;
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

    public EventBasedNetListener networkListener;
    public NetManager client;

    private string host = "localhost";
    private int port;
    private string password = "ttt";

    public bool isHost = false;

    public bool stopConnection = false;

    int maxPlayers = 100;

    Color currentColor;

    public GameObject theCube;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        UnityThread.initUnityThread();
    }

    public void InitGame(string hostaddress, int port, bool isHost)
    {
        stopConnection = false;
        this.isHost = isHost;
        this.host = hostaddress == "" ? "localhost" : hostaddress;
        this.port = port;

        currentColor = new Color(Random.value, Random.value, Random.value);
        
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
        print("receivepackage event: " + msgid);
        switch (msgid)
        {
            case 1: 
                {
                    ColorPacket result = reader.GetBytesWithLength().ToStructure<ColorPacket>();
                    currentColor = result.ToColor();
                    UnityThread.executeInUpdate(() => {
                        RefreshColor();
                    });
                }
                break;
        }
    }

    public void RefreshColor()
    {
        print("refreshing cube color with: " + currentColor);
        theCube.GetComponent<Renderer>().material.color = currentColor;
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
        //NetDataWriter writer = new NetDataWriter();
        //writer.Put((ushort)1);
        //writer.PutBytesWithLength()
        //peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendColorUpdate(Color color)
    {
        currentColor = color;
        RefreshColor();
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort)1);
        writer.PutBytesWithLength(new ColorPacket(color.r, color.g, color.b).ToByteArray());
        Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void Send(NetDataWriter writer, DeliveryMethod method)
    {
        client?.SendToAll(writer, method);
    }

    private void SendOthers(NetPeer peer, NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        foreach (NetPeer netPeer in client.ConnectedPeerList)
        {
            if (peer == netPeer) continue;

            netPeer.Send(writer, deliveryMethod);
        }
    }

    private void SendOthers(NetPeer peer, byte[] data, DeliveryMethod deliveryMethod)
    {
        foreach (NetPeer netPeer in client.ConnectedPeerList)
        {
            if (peer == netPeer) continue;

            netPeer.Send(data, deliveryMethod);
        }
    }
    private void OnApplicationQuit()
    {
        client.Stop();
    }

}
