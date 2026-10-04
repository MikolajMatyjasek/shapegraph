using Galaretka.ShapeGraph.Components;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Combine;
using Galaretka.ShapeGraph.Data.Nodes.Generators;
using Galaretka.ShapeGraph.Data.Nodes.Modifiers;
using Galaretka.ShapeGraph.Data.Nodes.Output;
using Galaretka.ShapeGraph.Data.Nodes.Style;
using Galaretka.ShapeGraph.Data.Patterns;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galaretka.ShapeGraph.Samples.Smoke
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProceduralShapeInstance))]
    public sealed class ShapeGraphSmokeDemo : MonoBehaviour
    {
        [Header("Instance")]
        [SerializeField]
        private int seed = 1337;

        [Header("Star branch")]
        [SerializeField]
        private float starOuter = 0.55f;
        [SerializeField]
        private float starInner = 0.22f;
        [SerializeField]
        [Min(3)]
        private int starPoints = 5;
        [SerializeField]
        private float starNoise = 0.028f;
        [SerializeField]
        private Color32 starFill = new Color32(220, 120, 90, 255);
        [SerializeField]
        private Color32 starOutline = new Color32(40, 18, 12, 255);
        [SerializeField]
        private float starOutlineThickness = 0.045f;

        [Header("Polygon branch")]
        [FormerlySerializedAs("hexTranslation")]
        [SerializeField]
        private Vector2 polygonTranslation = new Vector2(-0.55f, 0.12f);
        [FormerlySerializedAs("hexRadius")]
        [SerializeField]
        private float polygonRadius = 0.36f;
        [SerializeField]
        [Min(3)]
        private int polygonSides = 6;
        [SerializeField]
        private Color32 polygonFill = new Color32(90, 170, 210, 255);
        [SerializeField]
        private Color32 polygonOutline = new Color32(12, 28, 40, 255);
        [SerializeField]
        private float polygonOutlineThickness = 0.04f;

        [Header("Ellipse branch")]
        [SerializeField]
        private Vector2 ellipseTranslation = new Vector2(0.55f, -0.08f);
        [SerializeField]
        private float ellipseRadiusX = 0.42f;
        [SerializeField]
        private float ellipseRadiusY = 0.24f;
        [SerializeField]
        [Min(3)]
        private int ellipseSegments = 48;
        [SerializeField]
        private Color32 ellipseFill = new Color32(140, 200, 120, 255);
        [SerializeField]
        private Color32 ellipseOutline = new Color32(16, 36, 18, 255);
        [SerializeField]
        private float ellipseOutlineThickness = 0.04f;

        private ProceduralShapeInstance instance;
        private ShapeGraphAsset graphAsset;

        private StarNode starNode;
        private NoiseDisplaceNode starNoiseNode;
        private FillStyleNode starFillNode;
        private OutlineStyleNode starOutlineNode;

        private RegularPolygonNode hexNode;
        private TransformNode hexTransformNode;
        private FillStyleNode hexFillNode;
        private OutlineStyleNode hexOutlineNode;

        private EllipseNode ellipseNode;
        private TransformNode ellipseTransformNode;
        private FillStyleNode ellipseFillNode;
        private OutlineStyleNode ellipseOutlineNode;

        private ComposeShapesNode composeNode;
        private MeshOutputNode outputNode;
        private bool built;

        private void OnEnable()
        {
            instance = GetComponent<ProceduralShapeInstance>();
            EnsureGraph();
            ApplyEmbeddedValues();
            BindToInstance();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled || !built) return;

            ApplyEmbeddedValues();
            BindToInstance();
        }

        private void OnDestroy() => Cleanup();

        [ContextMenu("Rebuild Graph")]
        private void RebuildGraphContextMenu()
        {
            Cleanup();
            built = false;
            EnsureGraph();
            ApplyEmbeddedValues();
            BindToInstance();
        }

        #region BuildGraph

        private void EnsureGraph()
        {
            if (built && graphAsset != null) return;

            BuildGraph();
            Wire();
            built = true;
        }

        private void BuildGraph()
        {
            graphAsset = CreateHidden<ShapeGraphAsset>("SmokeStyleDomainGraph");

            starNode = CreateHidden<StarNode>("Star");
            starNoiseNode = CreateHidden<NoiseDisplaceNode>("StarNoise");
            starFillNode = CreateHidden<FillStyleNode>("StarFill");
            starOutlineNode = CreateHidden<OutlineStyleNode>("StarOutline");

            hexNode = CreateHidden<RegularPolygonNode>("Hex");
            hexTransformNode = CreateHidden<TransformNode>("HexTransform");
            hexFillNode = CreateHidden<FillStyleNode>("HexFill");
            hexOutlineNode = CreateHidden<OutlineStyleNode>("HexOutline");

            ellipseNode = CreateHidden<EllipseNode>("Ellipse");
            ellipseTransformNode = CreateHidden<TransformNode>("EllipseTransform");
            ellipseFillNode = CreateHidden<FillStyleNode>("EllipseFill");
            ellipseOutlineNode = CreateHidden<OutlineStyleNode>("EllipseOutline");

            composeNode = CreateHidden<ComposeShapesNode>("Compose");
            outputNode = CreateHidden<MeshOutputNode>("MeshOutput");

            graphAsset.AddNodeDirectly(starNode);
            graphAsset.AddNodeDirectly(starNoiseNode);
            graphAsset.AddNodeDirectly(starFillNode);
            graphAsset.AddNodeDirectly(starOutlineNode);
            graphAsset.AddNodeDirectly(hexNode);
            graphAsset.AddNodeDirectly(hexTransformNode);
            graphAsset.AddNodeDirectly(hexFillNode);
            graphAsset.AddNodeDirectly(hexOutlineNode);
            graphAsset.AddNodeDirectly(ellipseNode);
            graphAsset.AddNodeDirectly(ellipseTransformNode);
            graphAsset.AddNodeDirectly(ellipseFillNode);
            graphAsset.AddNodeDirectly(ellipseOutlineNode);
            graphAsset.AddNodeDirectly(composeNode);
            graphAsset.AddNodeDirectly(outputNode);
        }

        private void Wire()
        {
            Connect(starNode, ShapeGeneratorNode.ShapeOut, starNoiseNode, ShapeModifierNode.ShapeIn);
            Connect(starNoiseNode, ShapeModifierNode.ShapeOut, starFillNode, FillStyleNode.ShapeIn);
            Connect(starFillNode, FillStyleNode.RegionOut, starOutlineNode, OutlineStyleNode.RegionIn);

            Connect(hexNode, ShapeGeneratorNode.ShapeOut, hexTransformNode, ShapeModifierNode.ShapeIn);
            Connect(hexTransformNode, ShapeModifierNode.ShapeOut, hexFillNode, FillStyleNode.ShapeIn);
            Connect(hexFillNode, FillStyleNode.RegionOut, hexOutlineNode, OutlineStyleNode.RegionIn);

            Connect(ellipseNode, ShapeGeneratorNode.ShapeOut, ellipseTransformNode, ShapeModifierNode.ShapeIn);
            Connect(ellipseTransformNode, ShapeModifierNode.ShapeOut, ellipseFillNode, FillStyleNode.ShapeIn);
            Connect(ellipseFillNode, FillStyleNode.RegionOut, ellipseOutlineNode, OutlineStyleNode.RegionIn);

            Connect(starOutlineNode, OutlineStyleNode.RegionOut, composeNode, ComposeShapesNode.RegionAIn);
            Connect(hexOutlineNode, OutlineStyleNode.RegionOut, composeNode, ComposeShapesNode.RegionBIn);
            Connect(ellipseOutlineNode, OutlineStyleNode.RegionOut, composeNode, ComposeShapesNode.RegionCIn);

            Connect(composeNode, ComposeShapesNode.PictureOut, outputNode, TerminalOutputNode.PictureIn);
        }

        private void Connect(ShapeNode from, PortId fromPort, ShapeNode to, PortId toPort)
        {
            if (!graphAsset.AddConnectionDirectly(new NodeConnection(from.Id, fromPort, to.Id, toPort)))
            {
                Debug.LogError($"[ShapeGraph] Smoke demo failed to connect {from.name} -> {to.name}.");
            }
        }

        #endregion

        #region Bind

        private void ApplyEmbeddedValues()
        {
            if (starNode == null) return;

            starNode.OuterRadius = starOuter;
            starNode.InnerRadius = starInner;
            starNode.Points = Mathf.Max(3, starPoints);
            starNode.RotationDegrees = -90f;
            starNoiseNode.Amplitude = starNoise;
            starFillNode.FillColor = starFill;
            starOutlineNode.OutlineColor = starOutline;
            starOutlineNode.Thickness = starOutlineThickness;

            hexNode.Sides = Mathf.Max(3, polygonSides);
            hexNode.Radius = polygonRadius;
            hexNode.RotationDegrees = 30f;
            hexTransformNode.Translation = polygonTranslation;
            hexTransformNode.RotationDegrees = -8f;
            hexTransformNode.PivotMode = TransformPivotMode.Centroid;
            hexFillNode.FillColor = polygonFill;
            hexOutlineNode.OutlineColor = polygonOutline;
            hexOutlineNode.Thickness = polygonOutlineThickness;

            ellipseNode.RadiusX = ellipseRadiusX;
            ellipseNode.RadiusY = ellipseRadiusY;
            ellipseNode.Segments = Mathf.Max(3, ellipseSegments);
            ellipseTransformNode.Translation = ellipseTranslation;
            ellipseTransformNode.PivotMode = TransformPivotMode.Centroid;
            ellipseFillNode.FillColor = ellipseFill;
            ellipseOutlineNode.OutlineColor = ellipseOutline;
            ellipseOutlineNode.Thickness = ellipseOutlineThickness;
        }

        private void BindToInstance()
        {
            if (instance == null)
            {
                instance = GetComponent<ProceduralShapeInstance>();
            }

            if (instance == null || graphAsset == null) return;

            instance.Graph = graphAsset;
            instance.Seed = seed;
            instance.Rebuild();
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            DestroyHidden(outputNode);
            DestroyHidden(composeNode);
            DestroyHidden(ellipseOutlineNode);
            DestroyHidden(ellipseFillNode);
            DestroyHidden(ellipseTransformNode);
            DestroyHidden(ellipseNode);
            DestroyHidden(hexOutlineNode);
            DestroyHidden(hexFillNode);
            DestroyHidden(hexTransformNode);
            DestroyHidden(hexNode);
            DestroyHidden(starOutlineNode);
            DestroyHidden(starFillNode);
            DestroyHidden(starNoiseNode);
            DestroyHidden(starNode);
            DestroyHidden(graphAsset);

            outputNode = null;
            composeNode = null;
            ellipseOutlineNode = null;
            ellipseFillNode = null;
            ellipseTransformNode = null;
            ellipseNode = null;
            hexOutlineNode = null;
            hexFillNode = null;
            hexTransformNode = null;
            hexNode = null;
            starOutlineNode = null;
            starFillNode = null;
            starNoiseNode = null;
            starNode = null;
            graphAsset = null;
            built = false;

            if (instance != null)
            {
                instance.Graph = null;
            }
        }

        private static T CreateHidden<T>(string objectName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = objectName;
            asset.hideFlags = HideFlags.HideAndDontSave;
            return asset;
        }

        private static void DestroyHidden(Object asset)
        {
            if (asset == null) return;

            if (Application.isPlaying)
            {
                Destroy(asset);
            }
            else
            {
                DestroyImmediate(asset);
            }
        }

        #endregion
    }
}
