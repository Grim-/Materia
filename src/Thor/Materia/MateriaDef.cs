using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Thor
{
    //public class MateriaDef : Def
    //{
    //    //how common is this
    //    public float commonality = 1f;
    //    public string texPath;
    //    //the slot type
    //    public MateriaTypeDef SlotType;
    //    //effects that equipping this materia to a weapon applies
    //    public List<MateriaEffect> effects;
    //    //list of valid materia that can be linked to this one
    //    public List<MateriaLinkBonus> links;

    //    public bool CanLinkWith(MateriaDef other)
    //    {
    //        return other != this &&
    //               this.links.Any(x => x.materia == other);
    //    }

    //    // Add method to create workers (or put this wherever you currently handle it)
    //    public List<MateriaWorker> CreateWorkers(ThingWithComps Parent)
    //    {
    //        var workers = new List<MateriaWorker>();
    //        foreach (var effect in effects)
    //        {
    //            MateriaWorker worker = (MateriaWorker)Activator.CreateInstance(effect.workerClass);
    //            worker.Initialize(effect);
    //            worker.ParentEquipment = Parent;
    //            workers.Add(worker);
    //        }
    //        return workers;
    //    }
    //}
    public class MateriaDef : Def
    {
        //how common is this
        public float commonality = 1f;
        public string texPath;
        //the slot type
        public MateriaTypeDef SlotType;
        //effects that equipping this materia to a weapon applies
        public List<MateriaEffectDef> effects;
        //list of valid materia that can be linked to this one
        public List<MateriaLinkBonus> links;

        public bool CanLinkWith(MateriaDef other)
        {
            return other != this &&
                   this.links.Any(x => x.materia == other);
        }
    }


}
