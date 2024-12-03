using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public partial struct BoxBase
    {
        public bool Overlaps(BoxBase other)
        {
            var c1 = other.XMax >= XMin;
            var c2 = other.XMin <= XMax;
            var c3 = other.YMax >= YMin;
            var c4 = other.YMin <= YMax;

            return c1 && c2 && c3 && c4;
        }
    }
}
