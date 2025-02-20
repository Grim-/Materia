using System;
using Verse;

namespace Materia
{
    public class MateriaDisplayDef : Def
    {
        public Type displayClass;

        public MateriaDisplayWorker CreateWorker()
        {
            if (!typeof(MateriaDisplayWorker).IsAssignableFrom(displayClass))
            {
                Log.Error($"Error in MateriaDisplayDef {defName}: {displayClass.Name} is not a valid MateriaDisplayWorker.");
                return null;
            }

            try
            {
                MateriaDisplayWorker worker = (MateriaDisplayWorker)Activator.CreateInstance(displayClass);
                return worker;
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating MateriaDisplayWorker of type {displayClass.Name}: {ex}");
                return null;
            }
        }
    }
}
