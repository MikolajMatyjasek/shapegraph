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
        private const string SessionAssetGuidKey = "ShapeGraph.Editor.BoundAssetGuid";

        private GraphDirtyTracker dirtyTracker;
        private GraphCommandService commands;
        private NodePreviewService previews;
        private ShapeGraphView graphView;
        private ParametersPane parametersPane;
        private NodeInspectorPane inspectorPane;
        private Image masterPreview;
        private ObjectField assetField;
        private IntegerField seedField;
        private FloatField timeField;
        private Button playButton;
        private Label emptyState;
        private ShapeGraphAsset boundAsset;
        private int previewSeed = 1337;
        private float previewTime;
        private bool timePlaying;
        private double lastPlayEditorTime;

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

            ShapeGraphAsset restored = TryRestoreSessionAsset();
            if (restored != null)
            {
                BindAsset(restored);
            }
            else if (boundAsset != null)
            {
                BindAsset(boundAsset);
            }

            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
            timePlaying = false;
            if (commands != null)
            {
                commands.GraphChanged -= OnGraphChanged;
            }
            previews?.Dispose();
            previews = null;
            graphView = null;
            commands = null;
            dirtyTracker = null;
            parametersPane = null;
            inspectorPane = null;
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
                {
                    EditorGUIUtility.PingObject(boundAsset);
                }
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

            timeField = new FloatField("Time") { value = previewTime };
            timeField.style.width = 120;
            timeField.RegisterValueChangedCallback(evt =>
            {
                previewTime = Mathf.Max(0f, evt.newValue);
                previews?.SetTime(previewTime);
            });
            toolbar.Add(timeField);

            playButton = new Button(ToggleTimePlay) { text = "Play" };
            toolbar.Add(playButton);

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

            parametersPane = new ParametersPane(commands);
            side.Add(parametersPane);

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
            PersistSessionAsset(asset);

            if (assetField != null && assetField.value != asset)
            {
                assetField.SetValueWithoutNotify(asset);
            }

            titleContent = new GUIContent(asset != null ? $"Shape Graph — {asset.name}" : "Shape Graph");

            if (commands == null || graphView == null || previews == null) return;

            commands.Bind(asset);
            previews.Bind(asset, previewSeed, previewTime);
            previews.SubscribeMaster(tex =>
            {
                if (masterPreview != null)
                {
                    masterPreview.image = tex;
                }
            });

            graphView.LoadFromAsset();
            emptyState.style.display = asset == null || asset.Nodes.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            inspectorPane?.SetSelection(null);
            parametersPane?.Refresh();
        }

        private void OnGraphChanged(GraphChangeKind kind)
        {
            previews?.NotifyDirtyFromTracker();
            parametersPane?.Refresh();

            if (kind == GraphChangeKind.Structure || kind == GraphChangeKind.Bound)
            {
                EditorApplication.delayCall += RefreshGraphView;
            }

            if (kind == GraphChangeKind.Structure || kind == GraphChangeKind.Properties)
            {
                ShapeGraphAsset graph = boundAsset;
                EditorApplication.delayCall += () => GraphCommandService.RebuildSceneInstances(graph);
            }
        }

        private void OnUndoRedo()
        {
            if (boundAsset == null) return;

            dirtyTracker?.BumpStructure();
            dirtyTracker?.InvalidateAll(boundAsset);
            EditorApplication.delayCall += () =>
            {
                RefreshGraphView();
                previews?.RequestRebuildAll();
                parametersPane?.Refresh();
                GraphCommandService.RebuildSceneInstances(boundAsset);
            };
        }

        private void RefreshGraphView()
        {
            if (graphView == null) return;

            graphView.LoadFromAsset();
            emptyState.style.display = boundAsset == null || boundAsset.Nodes.Count == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void ToggleTimePlay()
        {
            timePlaying = !timePlaying;
            playButton.text = timePlaying ? "Pause" : "Play";
            lastPlayEditorTime = EditorApplication.timeSinceStartup;
        }

        private void OnEditorUpdate()
        {
            if (!timePlaying || previews == null)
                return;

            double now = EditorApplication.timeSinceStartup;
            float dt = (float)(now - lastPlayEditorTime);
            lastPlayEditorTime = now;
            previewTime += Mathf.Max(0f, dt);
            timeField?.SetValueWithoutNotify(previewTime);
            previews.SetTime(previewTime);
        }

        private static void PersistSessionAsset(ShapeGraphAsset asset)
        {
            if (asset == null)
            {
                SessionState.EraseString(SessionAssetGuidKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrEmpty(guid))
            {
                SessionState.SetString(SessionAssetGuidKey, guid);
            }
        }

        private static ShapeGraphAsset TryRestoreSessionAsset()
        {
            string guid = SessionState.GetString(SessionAssetGuidKey, string.Empty);
            if (string.IsNullOrEmpty(guid)) return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            return AssetDatabase.LoadAssetAtPath<ShapeGraphAsset>(path);
        }

        private static void LoadStyles(VisualElement root)
        {
            const string packagePath = "Packages/com.galaretka.shapegraph/Editor/Graph/Theme/ShapeGraphEditor.uss";
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(packagePath);
            if (sheet == null)
            {
                string[] guids = AssetDatabase.FindAssets("ShapeGraphEditor t:StyleSheet");
                if (guids != null && guids.Length > 0)
                {
                    sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (sheet != null)
            {
                root.styleSheets.Add(sheet);
            }
        }
    }
}
