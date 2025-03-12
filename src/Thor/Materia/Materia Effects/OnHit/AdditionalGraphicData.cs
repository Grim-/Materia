using UnityEngine;
using Verse;

namespace Materia
{
    public class AdditionalGraphicData
    {
        public GraphicData graphic;
        public Vector3 offset = Vector3.zero;
        public AltitudeLayer altitude = AltitudeLayer.MoteOverhead;
        public FloatRange extraRotation = new FloatRange(0,0);
    }
}