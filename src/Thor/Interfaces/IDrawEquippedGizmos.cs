using System.Collections.Generic;
using Verse;

namespace Thor
{
    public interface IDrawEquippedGizmos
    {
        IEnumerable<Gizmo> GetEquippedGizmos();
    }
}
