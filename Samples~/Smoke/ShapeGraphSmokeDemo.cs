using Galaretka.ShapeGraph.Components;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Data.Nodes.Combine;
using Galaretka.ShapeGraph.Data.Nodes.Generators;
using Galaretka.ShapeGraph.Data.Nodes.Mesh;
using Galaretka.ShapeGraph.Data.Nodes.Modifiers;
using Galaretka.ShapeGraph.Data.Nodes.Output;
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

        [Header("Mesh look")]
        [SerializeField] 
        private Color32 fillColor = new Color32(160, 170, 190, 255);
        [SerializeField] 
        private Color32 outlineColor = new Color32(20, 24, 36, 255);
        [SerializeField] 
        private float outlineThickness = 0.05f;

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

        [Header("Circle branch")]
        [SerializeField] 
        private Vector2 circleTranslation = new Vector2(0.55f, -0.08f);
        [SerializeField] 
        private float circleRadius = 0.32f;
        [SerializeField] 
        [Min(3)] 
        private int circleSegments = 48;

        private ProceduralShapeInstance instance;
        private ShapeGraphAsset graphAsset;

        private StarNode starNode;
        private NoiseDisplaceNode starNoiseNode;

        private RegularPolygonNode hexNode;
        private TransformNode hexTransformNode;

        private CircleNode circleNode;
        private TransformNode circleTransformNode;

        private UnionShapesNode unionNode;
        private FillMeshNode fillNode;
        private OutlineMeshNode outlineNode;
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
            graphAsset = CreateHidden<ShapeGraphAsset>("SmokeDualDomainGraph");

            starNode = CreateHidden<StarNode>("Star");
            starNoiseNode = CreateHidden<NoiseDisplaceNode>("StarNoise");

            hexNode = CreateHidden<RegularPolygonNode>("Hex");
            hexTransformNode = CreateHidden<TransformNode>("HexTransform");

            circleNode = CreateHidden<CircleNode>("Circle");
            circleTransformNode = CreateHidden<TransformNode>("CircleTransform");

            unionNode = CreateHidden<UnionShapesNode>("Union");
            fillNode = CreateHidden<FillMeshNode>("FillMesh");
            outlineNode = CreateHidden<OutlineMeshNode>("OutlineMesh");
            outputNode = CreateHidden<MeshOutputNode>("MeshOutput");

            graphAsset.AddNodeDirectly(starNode);
            graphAsset.AddNodeDirectly(starNoiseNode);
            graphAsset.AddNodeDirectly(hexNode);
            graphAsset.AddNodeDirectly(hexTransformNode);
            graphAsset.AddNodeDirectly(circleNode);
            graphAsset.AddNodeDirectly(circleTransformNode);
            graphAsset.AddNodeDirectly(unionNode);
            graphAsset.AddNodeDirectly(fillNode);
            graphAsset.AddNodeDirectly(outlineNode);
            graphAsset.AddNodeDirectly(outputNode);
        }

        private void Wire()
        {
            // Geometry domain
            Connect(starNode, ShapeGeneratorNode.ShapeOut, starNoiseNode, ShapeModifierNode.ShapeIn);
            Connect(hexNode, ShapeGeneratorNode.ShapeOut, hexTransformNode, ShapeModifierNode.ShapeIn);
            Connect(circleNode, ShapeGeneratorNode.ShapeOut, circleTransformNode, ShapeModifierNode.ShapeIn);

            Connect(starNoiseNode, ShapeModifierNode.ShapeOut, unionNode, UnionShapesNode.ShapeAIn);
            Connect(hexTransformNode, ShapeModifierNode.ShapeOut, unionNode, UnionShapesNode.ShapeBIn);
            Connect(circleTransformNode, ShapeModifierNode.ShapeOut, unionNode, UnionShapesNode.ShapeCIn);

            // Bridge + mesh domain
            Connect(unionNode, UnionShapesNode.ShapeOut, fillNode, FillMeshNode.ShapeIn);
            Connect(fillNode, FillMeshNode.MeshOut, outlineNode, OutlineMeshNode.MeshIn);
            Connect(outlineNode, OutlineMeshNode.MeshOut, outputNode, TerminalOutputNode.MeshIn);
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

            hexNode.Sides = Mathf.Max(3, polygonSides);
            hexNode.Radius = polygonRadius;
            hexNode.RotationDegrees = 30f;
            hexTransformNode.Translation = polygonTranslation;
            hexTransformNode.RotationDegrees = -8f;
            hexTransformNode.PivotMode = TransformPivotMode.Centroid;

            circleNode.Radius = circleRadius;
            circleNode.Segments = Mathf.Max(3, circleSegments);
            circleTransformNode.Translation = circleTranslation;
            circleTransformNode.PivotMode = TransformPivotMode.Centroid;

            fillNode.FillColor = fillColor;
            outlineNode.OutlineColor = outlineColor;
            outlineNode.Thickness = outlineThickness;
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
            DestroyHidden(outlineNode);
            DestroyHidden(fillNode);
            DestroyHidden(unionNode);
            DestroyHidden(circleTransformNode);
            DestroyHidden(circleNode);
            DestroyHidden(hexTransformNode);
            DestroyHidden(hexNode);
            DestroyHidden(starNoiseNode);
            DestroyHidden(starNode);
            DestroyHidden(graphAsset);

            outputNode = null;
            outlineNode = null;
            fillNode = null;
            unionNode = null;
            circleTransformNode = null;
            circleNode = null;
            hexTransformNode = null;
            hexNode = null;
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
