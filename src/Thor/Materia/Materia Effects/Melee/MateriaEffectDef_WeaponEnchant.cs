using System.Collections.Generic;

namespace Materia
{
    public class MateriaEffectDef_WeaponEnchant : MateriaEffectDef
    {
        public List<AdditionalGraphicData> graphics;

        public MateriaEffectDef_WeaponEnchant()
        {
            workerClass = typeof(MateriaEffect_WeaponEnchant);
        }
    }

    public class MateriaEffect_WeaponEnchant : MateriaWorker
    {
        MateriaEffectDef_WeaponEnchant Def => (MateriaEffectDef_WeaponEnchant)def;
        public override void PostDraw()
        {
            base.PostDraw();


           CompDrawAdditionalGraphics.DrawAdditionalGraphics(Def.graphics, this.EquippingPawn);
        }
    }
}