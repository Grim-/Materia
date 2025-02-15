using RimWorld;
using System.Collections.Generic;

namespace Thor
{
    public interface IStatProvider
    {
        IEnumerable<StatModifier> GetStatOffsets(StatDef stat);
        IEnumerable<StatModifier> GetStatFactors(StatDef stat);
        string GetExplanation(StatDef stat);
    }
}
