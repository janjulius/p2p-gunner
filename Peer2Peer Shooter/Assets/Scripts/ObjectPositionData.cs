using System;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Packets.GamePackets
{
    [Serializable]
    public struct ObjectPositionData
    {
        public uint lastPacketId;

        public Position position;
        public Rotation rotation;


        public ObjectPositionData(Position position, Rotation rotation)
        {
            this.position = position;
            this.rotation = rotation;
            lastPacketId = 0;
        }
    }
}