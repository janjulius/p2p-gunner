using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets.Scripts.Extensions;
using Assets.Scripts.Packets.GamePackets;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class NetworkObjectData
{
    public int networkId;
    public ObjectData objectData;
    public INetworkObject networkObject;
    private Vector3 oldPosition;
    private Quaternion oldRotation;
    private DateTime LastPacketReceived = DateTime.UtcNow;
    private DateTime NewPacketReceived = DateTime.UtcNow;
    public TimeSpan TimeBetweenPackets = new TimeSpan();

    public NetworkObjectData(int networkId, ObjectData objectData)
    {
        this.networkId = networkId;
        this.objectData = objectData;
    }

    public void ReadData(ObjectPositionData data)
    {
        if (data.lastPacketId > objectData.positionData.lastPacketId)
        {
            LastPacketReceived = NewPacketReceived;
            NewPacketReceived = DateTime.UtcNow;
            objectData.positionData = data;

            TimeBetweenPackets = NewPacketReceived - LastPacketReceived;
            //Debug.Log((NewPacketReceived - LastPacketReceived).TotalMilliseconds);
        }
    }

    public void WriteData(NetDataWriter writer)
    {
        writer.Put((ushort) 103);
        writer.Put(objectData.objectId);
        writer.PutBytesWithLength(objectData.positionData.ToByteArray());

        objectData.positionData.lastPacketId++;

        oldPosition = GetPosition();
        oldRotation = GetRotation();
    }


    public Quaternion GetRotation()
    {
        return objectData.positionData.rotation.ToQuaternion();
    }

    public void SetRotation(Quaternion rotation)
    {
        objectData.positionData.rotation.x = rotation.x;
        objectData.positionData.rotation.y = rotation.y;
        objectData.positionData.rotation.z = rotation.z;
        objectData.positionData.rotation.w = rotation.w;
    }

    public Vector3 GetPosition()
    {
        return objectData.positionData.position.ToVector3();
    }

    public void SetPosition(Vector3 position)
    {
        objectData.positionData.position.x = position.x;
        objectData.positionData.position.y = position.y;
        objectData.positionData.position.z = position.z;
    }

    public virtual bool CheckChanges()
    {
        return GetPosition() == oldPosition && GetRotation() == oldRotation;
    }


    public void Instantiate(GameObject obj, ushort lId = 60000)
    {
        if (NetworkManager.Instance.MyNetworkId != networkId)
            networkObject = GameObject.Instantiate(obj, GetPosition(), GetRotation()).GetComponent<INetworkObject>();
        else
        {
            NetworkManager.Instance.ObjectsInWaiting[lId].SetActive(true);
            networkObject = NetworkManager.Instance.ObjectsInWaiting[lId].GetComponent<INetworkObject>();
            NetworkManager.Instance.ObjectsInWaiting.Remove(lId);
        }


        networkObject.Init(objectData.playerId, objectData.objectId, networkId);
    }

    public void SendObjectData(NetPeer peer, NetDataWriter writer)
    {
        writer.Put((ushort)102);
        writer.Put(peer.Id);
        writer.PutBytesWithLength(objectData.ToByteArray());
    }
}