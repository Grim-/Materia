using System.Collections.Generic;
using Verse;

namespace Materia
{
    public interface IDrawEquippedGizmos
    {
        IEnumerable<Gizmo> GetEquippedGizmos();
    }
}
