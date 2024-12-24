using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Pushbox
    {
        public FP XMin { get { return RectPos.X - RectWH.X / 2; } }
        public FP XMax { get { return RectPos.X + RectWH.X / 2; } }
        public FP YMin { get { return RectPos.Y; } }
        public FP YMax { get { return RectPos.Y + RectWH.Y; } }
        
        public bool Overlaps(Pushbox other)
        {
            var c1 = other.XMax >= XMin;
            var c2 = other.XMin <= XMax;
            var c3 = other.YMax >= YMin;
            var c4 = other.YMin <= YMax;

            return c1 && c2 && c3 && c4;
        }
    }
    
    public unsafe partial struct Hurtbox
    {
        public FP XMin { get { return RectPos.X - RectWH.X / 2; } }
        public FP XMax { get { return RectPos.X + RectWH.X / 2; } }
        public FP YMin { get { return RectPos.Y; } }
        public FP YMax { get { return RectPos.Y + RectWH.Y; } }
        
        public bool Overlaps(Hitbox other)
        {
            var c1 = other.XMax >= XMin;
            var c2 = other.XMin <= XMax;
            var c3 = other.YMax >= YMin;
            var c4 = other.YMin <= YMax;

            return c1 && c2 && c3 && c4;
        }
    }
    
    public unsafe partial struct Hitbox
    {
        public FP XMin { get { return RectPos.X - RectWH.X / 2; } }
        public FP XMax { get { return RectPos.X + RectWH.X / 2; } }
        public FP YMin { get { return RectPos.Y; } }
        public FP YMax { get { return RectPos.Y + RectWH.Y; } }
        
        public bool Overlaps(Hurtbox other)
        {
            var c1 = other.XMax >= XMin;
            var c2 = other.XMin <= XMax;
            var c3 = other.YMax >= YMin;
            var c4 = other.YMin <= YMax;

            return c1 && c2 && c3 && c4;
        }
    }
}
