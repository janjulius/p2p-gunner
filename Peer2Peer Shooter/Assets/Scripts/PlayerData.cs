using System;
using System.Runtime.InteropServices;
using Assets.Scripts.Extensions;
using Assets.Scripts.Packets.GamePackets;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;


public class PlayerData
{
    public PlayerInfo playerInfo;
    public GameObject CharacterObject;

    public PlayerData(PlayerInfo info)
    {
        playerInfo = info;
    }

    public void SendUpdatedData()
    {
        var writer = new NetDataWriter();
        writer.Put((ushort) 4);
        writer.PutBytesWithLength(playerInfo.ToByteArray());
        NetworkManager.Instance.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void ReadPlayerData(PlayerInfo playerData)
    {
        playerInfo = playerData;
    }
}