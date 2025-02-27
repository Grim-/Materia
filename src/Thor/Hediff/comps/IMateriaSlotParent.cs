namespace Materia
{
    public interface IMateriaSlotParent
    {
        MateriaSlot MateriaSlot { get; }

        void SetMateriaSlot(MateriaSlot materiaSlot);
    }
}
