using RimWorld;
using Verse;

namespace Thor
{
    public class StatMod : IExposable
    {
        public StatDef stat;
        public float value;

        public StatMod() 
        { 
        
        }

        public StatMod(StatDef stat, float value)
        {
            this.stat = stat;
            this.value = value;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref stat, "stat");
            Scribe_Values.Look(ref value, "value");
        }
    }

}