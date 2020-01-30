using System;
using UnityEngine;

namespace Assets.Scripts.Packets.GamePackets
{
    [Serializable]
    public struct Rotation
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Rotation(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}