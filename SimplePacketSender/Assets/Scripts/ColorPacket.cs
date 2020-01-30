using System;
using UnityEngine;

namespace Assets.Scripts.Packets.GamePackets
{
    [Serializable]
    public struct ColorPacket
    {
        public float r;
        public float g;
        public float b;

        public ColorPacket(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color ToColor()
        {
            return new Color(r, g, b);
        }
    }
}