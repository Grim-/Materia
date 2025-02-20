using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Materia
{
    public class PawnOverlayNodeProperties : PawnRenderNodeProperties
    {
        public Color overlayColor = Color.white;
        public float overlayAlpha = 1f;
        public Vector3 offset = Vector3.zero;
        public float layerOffset = 0.1f;
        public float northFacingLayerOffset = 10f;
        public Vector3 eastOffset = new Vector3(-1, 0, 0);
        public Vector3 westOffset = new Vector3(1, 0, 0);
        public GraphicData graphicData;
        public bool useBodyTypeVariants = false;

        public PawnOverlayNodeProperties()
        {
            this.nodeClass = typeof(PawnOverlayNode);
            this.workerClass = typeof(PawnOverlayNodeWorker);
        }
    }

    public class PawnOverlayNode : PawnRenderNode
    {
        public new PawnOverlayNodeProperties Props => (PawnOverlayNodeProperties)props;

        public PawnOverlayNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {

        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            string texPath = Props.useBodyTypeVariants ? GetBodyTypeTexPath(pawn) : Props.graphicData.texPath;
            return GraphicDatabase.Get<Graphic_Multi>(
                texPath,
                ShaderFor(pawn),
                Props.graphicData.drawSize,
                ColorFor(pawn),
                Props.graphicData.colorTwo
            );
        }
        private string GetBodyTypeTexPath(Pawn pawn)
        {
            string basePath = Props.graphicData.texPath;
            if (pawn?.story?.bodyType == null) 
                return basePath + "Male";

            return basePath + $"_{pawn.story.bodyType.defName}";
        }
    }

    public class PawnOverlayNodeWorker : PawnRenderNodeWorker
    {
        protected int currentTicks = 0;
        protected int colorShiftTicks = 300;
        protected Color currentColor;
        protected Color targetColor;

        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            PawnOverlayNode overlayNode = node as PawnOverlayNode;
            if (overlayNode == null || overlayNode.Graphic == null) return;

            Mesh mesh = node.GetMesh(parms);
            if (mesh == null) return;

            Material material = overlayNode.GraphicFor(parms.pawn).MatAt(parms.facing);
            if (material == null) return;


            Vector3 drawLoc;
            Vector3 pivot;
            Quaternion quat;
            Vector3 scale;

            Vector3 offset = this.OffsetFor(node, parms, out pivot);
            node.GetTransform(parms, out drawLoc, out _, out quat, out scale);
            drawLoc += offset;

            if (overlayNode.Props.graphicData != null)
            {
                scale = new Vector3(overlayNode.Props.graphicData.drawSize.x, 1f, overlayNode.Props.graphicData.drawSize.y);
            }

            PawnGraphicDrawRequest request = new PawnGraphicDrawRequest(node, mesh, material);
            request.preDrawnComputedMatrix = Matrix4x4.TRS(drawLoc, quat, scale);
            requests.Add(request);
        }

        public override MaterialPropertyBlock GetMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms)
        {
            var matPropBlock = base.GetMaterialPropertyBlock(node, material, parms);
            if (matPropBlock == null)
                return null;

            var overlayNode = node as PawnOverlayNode;
            if (overlayNode == null)
                return matPropBlock;

            matPropBlock.SetColor(ShaderPropertyIDs.Color, parms.tint * overlayNode.Props.overlayColor);
            return matPropBlock;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 baseOffset = base.OffsetFor(node, parms, out pivot);

            if (node is PawnOverlayNode overlayNode)
            {
                return baseOffset + overlayNode.Props.offset;
            }

            return baseOffset;
        }

        protected override Vector3 PivotFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Vector3 basePivot = base.PivotFor(node, parms);
            if (node is PawnOverlayNode overlayNode)
            {
                Vector3 customPivotAdjustment = Vector3.zero;
                return basePivot + customPivotAdjustment;
            }

            return basePivot;
        }
        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnOverlayNode overlayNode && overlayNode.Props.graphicData != null)
            {
                Vector2 baseSize = overlayNode.Props.graphicData.drawSize;
                return new Vector3(
                    baseSize.x,
                    1f,
                    baseSize.y
                );
            }
            return base.ScaleFor(node, parms);
        }
        public override float LayerFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (node is PawnOverlayNode overlayNode)
            {
                float baseLayer = base.LayerFor(node, parms);
                float southFacingAdjustment = (parms.facing == Rot4.North ? overlayNode.Props.northFacingLayerOffset : 0);

                return baseLayer + overlayNode.Props.layerOffset + southFacingAdjustment;
            }
            return base.LayerFor(node, parms);
        }
    }
}
