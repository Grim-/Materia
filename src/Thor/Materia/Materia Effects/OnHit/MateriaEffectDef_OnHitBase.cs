namespace Materia
{
    public class MateriaEffectDef_OnHitBase : MateriaEffectDef
    {
        public OnHitMode hitMode = OnHitMode.Melee;

        public string attackType => hitMode == OnHitMode.Range ? "ranged" : "melee";
    }


}