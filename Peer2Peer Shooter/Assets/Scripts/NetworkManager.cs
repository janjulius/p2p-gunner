using Assets.Scripts.Extensions;
using Assets.Scripts.Packets.GamePackets;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
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
    public bool registered = false;

    private string host = "localhost";
    private int port;
    private string password;
    private int maxPlayers;
    public GameObject PlayerObject;

    public NetworkInfo NetworkInfo;
    
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
    public Dictionary<int, NetworkObjectData> NetworkObjects = new Dictionary<int, NetworkObjectData>();
    public Dictionary<ushort, GameObject> ObjectsInWaiting = new Dictionary<ushort, GameObject>();

    public int MyNetworkId;
    System.Random rand = new System.Random();

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
        
        myName = "test " + UnityEngine.Random.value;
        myColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        print($"Initgame: Name:{myName} color:{myColor.ToString()}");
        if (isHost)
        {
            RegisterPlayer(null);
        }
        
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
        if (!isHost && !registered)
        {
            writer.Put((ushort)1);

            writer.Put((int)client?.FirstPeer?.Id);
            int newPlayerId;
            do
            {
                newPlayerId = rand.Next(1000000, 9999999);
            } while (Players.ContainsKey(newPlayerId));

            writer.Put(newPlayerId);
            writer.Put(isHost);
            Send(writer, DeliveryMethod.ReliableOrdered);
            registered = true;
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
            case 1: //register to host
                {
                    RegisterPlayer(reader);
                }
                break;
            case 101: //create networkObject
                {
                    int objectId;
                    int lobjectId = reader.GetUShort();
                    ObjectData objectData = reader.GetBytesWithLength().ToStructure<ObjectData>();

                    do
                    {
                        objectId = UnityEngine.Random.Range(1000000, 9999999);
                    } while (NetworkObjects.ContainsKey(objectId));

                    objectData.objectId = objectId;

                    var netObj = new NetworkObjectData(objectId, objectData);
                    NetworkObjects.Add(objectId, netObj);

                    netObj.SendObjectData(peer, writer);
                    writer.Put(lobjectId);
                    client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
                }
                break;
        }
    }

    private void RegisterPlayer(NetPacketReader reader)
    {
        if (isHost)
        {
            var pinf = new PlayerInfo(MyNetworkId, rand.Next(0, 10000000), isHost, "test", new ColorPacket(myColor.r, myColor.g, myColor.b));
            InitPlayer(pinf);
        }
        else
        {
            NetDataWriter writer = new NetDataWriter();
            var networkId = reader.GetInt();
            var playerId = reader.GetInt();
            var ishost = reader.GetBool();
            var playerColor = myColor;
            var pName = myName;
            MyNetworkId = networkId;

            var playerInfo = new PlayerInfo(networkId, playerId, ishost, pName, new ColorPacket(playerColor.r, playerColor.g, playerColor.b));

            InitPlayer(playerInfo);

            writer.Put((ushort)1);
            writer.PutBytesWithLength(playerInfo.ToByteArray());

            Send(writer, DeliveryMethod.ReliableOrdered);

            writer.Reset();
        }
    }

    private void InitPlayer(PlayerInfo playerInfo)
    {
        var p = new PlayerData(playerInfo);
        Players.Add(playerInfo.playerId, p);
        var obj = InstantiateNetworkObject("Player", playerInfo.playerId, new Vector3(0, 0, 0), Quaternion.identity);
        obj.GetComponent<MeshRenderer>().materials[0].color = new Color(playerInfo.playerColor.r, playerInfo.playerColor.g, playerInfo.playerColor.b);
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
        writer.Put((ushort)2);
        RegisterPlayer(null);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public GameObject InstantiateNetworkObject(string prefabName, int playerId, Vector3 pos, Quaternion rot)
    {
        ushort i = 0;
        GameObject obj = null;
        if (Thread.CurrentThread == mainThread)
        {
            obj = Instantiate(NetworkInfo.GetNetworkPrefabByName(prefabName), pos, rot);
            //obj.SetActive(false);
            while (ObjectsInWaiting.ContainsKey(i))
                i++;

            ObjectsInWaiting.Add(i, obj);
        }
        else
        {
            UnityThread.executeInUpdate(
                () =>
                {
                    print("search for " + prefabName);
                    foreach(var p in NetworkInfo.networkPrefabs)
                    {
                        print(p.name);
                    }
                    print(NetworkInfo.GetNetworkPrefabByName(prefabName) + " " + NetworkInfo.networkPrefabs.Length);
                    obj = Instantiate(NetworkInfo.GetNetworkPrefabByName(prefabName), pos, rot);
                    //obj.SetActive(false);
                    while (ObjectsInWaiting.ContainsKey(i))
                        i++;

                    ObjectsInWaiting.Add(i, obj);
                });
        }

        ObjectData data = new ObjectData(playerId, -1, NetworkInfo.GetNetworkPrefabId(prefabName), new Position(pos.x, pos.y, pos.z), new Rotation(rot.x, rot.y, rot.z, rot.w));
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort)101);
        writer.Put(i);
        writer.PutBytesWithLength(data.ToByteArray());
        Send(writer, DeliveryMethod.ReliableOrdered);

        return obj;
    }

    public void DestroyNetworkObject(int objectId)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort)102);
        writer.Put(objectId);
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
