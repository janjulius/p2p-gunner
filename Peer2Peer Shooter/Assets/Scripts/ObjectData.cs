using System;

namespace Assets.Scripts.Packets.GamePackets
{
    [Serializable]
    public struct ObjectData
    {
        public int playerId;
        public int objectId;
        public int objectType;

        public ObjectPositionData positionData;

        public ObjectData(int playerId, int objectId, int objectType, Position pos, Rotation rot)
        {
            this.playerId = playerId;
            this.objectId = objectId;
            this.objectType = objectType;

            positionData = new ObjectPositionData(pos, rot);
        }
    }
}