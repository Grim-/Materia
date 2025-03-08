using System.Collections.Generic;
using Verse;

namespace Materia
{
    public class CompProperties_DrawAdditionalGraphics : CompProperties
    {
        public List<AdditionalGraphicData> graphics;

        public CompProperties_DrawAdditionalGraphics()
        {
            compClass = typeof(CompDrawAdditionalGraphics);
        }  
    }

    public class CompDrawAdditionalGraphics : ThingComp
    {
        private CompProperties_DrawAdditionalGraphics Props
        {
            get
            {
                return (CompProperties_DrawAdditionalGraphics)this.props;
            }
        }

        public override void PostDraw()
        {
            if (Props.graphics != null)
            {
                DrawAdditionalGraphics(Props.graphics, this.parent);
            }

          
        }


        public static void DrawAdditionalGraphics(List<AdditionalGraphicData> addtionalGraphics, Thing parentThing)
        {
            if (addtionalGraphics != null && parentThing != null)
            {

                foreach (AdditionalGraphicData graphicData in addtionalGraphics)
                {
                    Log.Message(graphicData);
                    Log.Message(graphicData.graphic);
                    Log.Message(graphicData.graphic.Graphic);
                    graphicData.graphic.Graphic.Draw(parentThing.DrawPos.SetToAltitude(graphicData.altitude) + graphicData.offset, parentThing.Rotation, parentThing, 0f);
                }
            }

        }
    }
}