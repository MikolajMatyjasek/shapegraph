using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Commands;
using Galaretka.ShapeGraph.Editor.Graph.Inspect;
using Galaretka.ShapeGraph.Editor.Graph.Model;
using Galaretka.ShapeGraph.Editor.Graph.Preview;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galaretka.ShapeGraph.Editor.Graph
{
    public sealed class ShapeGraphEditorWindow : EditorWindow
    {
        private GraphDirtyTracker dirtyTracker;
        private GraphCommandService commands;
        private NodePreviewService previews;
        private ShapeGraphView graphView;
        private NodeInspectorPane inspectorPane;
        private Image masterPreview;
        private ObjectField assetField;
        private IntegerField seedField;
        private Label emptyState;
        private ShapeGraphAsset boundAsset;
        private int previewSeed = 1337;

        [MenuItem("Window/Galaretka/Shape Graph Editor")]
        public static void Open()
        {
            var window = GetWindow<ShapeGraphEditorWindow>();
            window.titleContent = new GUIContent("Shape Graph");
            window.Focus();
        }

        public static void OpenAsset(ShapeGraphAsset asset)
        {
            var window = GetWindow<ShapeGraphEditorWindow>();
            window.titleContent = new GUIContent("Shape Graph");
            window.BindAsset(asset);
            window.Focus();
        }

        private void OnEnable()
        {
            dirtyTracker = new GraphDirtyTracker();
            commands = new GraphCommandService(dirtyTracker);
            previews = new NodePreviewService(dirtyTracker);
            commands.GraphChanged += OnGraphChanged;

            BuildUi();
            if (boundAsset != null)
                BindAsset(boundAsset);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            if (commands != null)
                commands.GraphChanged -= OnGraphChanged;
            previews?.Dispose();
            previews = null;
            graphView = null;
            commands = null;
            dirtyTracker = null;
        }

        private void BuildUi()
        {
            rootVisualElement.Clear();
            rootVisualElement.AddToClassList("shape-graph-root");
            LoadStyles(rootVisualElement);

            var toolbar = new VisualElement();
            toolbar.AddToClassList("toolbar");

            assetField = new ObjectField("Asset")
            {
                objectType = typeof(ShapeGraphAsset),
                allowSceneObjects = false
            };
            assetField.style.minWidth = 220;
            assetField.RegisterValueChangedCallback(evt => BindAsset(evt.newValue as ShapeGraphAsset));
            toolbar.Add(assetField);

            var ping = new Button(() =>
            {
                if (boundAsset != null)
                    EditorGUIUtility.PingObject(boundAsset);
            }) { text = "Ping" };
            toolbar.Add(ping);

            seedField = new IntegerField("Seed") { value = previewSeed };
            seedField.style.width = 120;
            seedField.RegisterValueChangedCallback(evt =>
            {
                previewSeed = evt.newValue;
                previews?.SetSeed(previewSeed);
            });
            toolbar.Add(seedField);

            toolbar.Add(new Button(() => graphView?.FrameAllNodes()) { text = "Frame" });
            toolbar.Add(new Button(() => previews?.RequestRebuildAll()) { text = "Rebuild Previews" });

            rootVisualElement.Add(toolbar);

            var body = new VisualElement();
            body.AddToClassList("body-row");

            var graphHost = new VisualElement();
            graphHost.AddToClassList("graph-host");
            graphView = new ShapeGraphView(commands, previews);
            graphView.SelectionChanged += node => inspectorPane?.SetSelection(node);
            graphHost.Add(graphView);

            emptyState = new Label("Create or open a Shape Graph asset.\nRight-click / Space to add nodes.");
            emptyState.AddToClassList("empty-state");
            graphHost.Add(emptyState);

            body.Add(graphHost);

            var side = new VisualElement();
            side.AddToClassList("side-column");
            inspectorPane = new NodeInspectorPane(commands);
            side.Add(inspectorPane);

            var masterPane = new VisualElement();
            masterPane.AddToClassList("master-preview-pane");
            masterPane.Add(new Label("Master Preview"));
            masterPreview = new Image { scaleMode = ScaleMode.ScaleToFit };
            masterPreview.AddToClassList("master-preview-image");
            masterPane.Add(masterPreview);
            side.Add(masterPane);

            body.Add(side);
            rootVisualElement.Add(body);
        }

        public void BindAsset(ShapeGraphAsset asset)
        {
            boundAsset = asset;
            if (assetField != null && assetField.value != asset)
                assetField.SetValueWithoutNotify(asset);

            titleContent = new GUIContent(asset != null ? $"Shape Graph — {asset.name}" : "Shape Graph");

            if (commands == null || graphView == null || previews == null)
                return;

            commands.Bind(asset);
            previews.Bind(asset, previewSeed);
            previews.SubscribeMaster(tex =>
            {
                if (masterPreview != null)
                    masterPreview.image = tex;
            });

            graphView.LoadFromAsset();
            emptyState.style.display = asset == null || asset.Nodes.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            inspectorPane.SetSelection(null);
        }

        private void OnGraphChanged(GraphChangeKind kind)
        {
            previews?.NotifyDirtyFromTracker();

            if (kind == GraphChangeKind.Structure || kind == GraphChangeKind.Bound)
            {
                EditorApplication.delayCall += RefreshGraphView;
            }
        }

        private void OnUndoRedo()
        {
            if (boundAsset == null)
                return;

            dirtyTracker?.BumpStructure();
            dirtyTracker?.InvalidateAll(boundAsset);
            EditorApplication.delayCall += () =>
            {
                RefreshGraphView();
                previews?.RequestRebuildAll();
            };
        }

        private void RefreshGraphView()
        {
            if (graphView == null)
                return;

            graphView.LoadFromAsset();
            emptyState.style.display = boundAsset == null || boundAsset.Nodes.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private static void LoadStyles(VisualElement root)
        {
            const string packagePath = "Packages/com.galaretka.shapegraph/Editor/Graph/Theme/ShapeGraphEditor.uss";
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(packagePath);
            if (sheet == null)
            {
                string[] guids = AssetDatabase.FindAssets("ShapeGraphEditor t:StyleSheet");
                if (guids != null && guids.Length > 0)
                    sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (sheet != null)
                root.styleSheets.Add(sheet);
        }
    }
}
