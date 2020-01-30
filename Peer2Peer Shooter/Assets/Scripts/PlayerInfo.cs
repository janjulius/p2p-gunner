using System;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Packets.GamePackets
{
    [Serializable]
    public struct PlayerInfo
    {
        public int networkId;
        public int playerId;
        public bool isHost;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string playerName;

        public ColorPacket playerColor;
        
        public PlayerInfo(int networkId, int playerId, bool isHost, string playerName, ColorPacket playerColor)
        {
            this.networkId = networkId;
            this.playerId = playerId;
            this.isHost = isHost;
            this.playerName = playerName;
            this.playerColor = playerColor;
        }
    }
}