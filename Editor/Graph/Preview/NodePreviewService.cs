using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using Galaretka.ShapeGraph.Data;
using Galaretka.ShapeGraph.Editor.Graph.Model;
using UnityEngine;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    public sealed class NodePreviewService : IDisposable
    {
        private const float TimeQuantStep = 1f / 30f;

        private readonly GraphDirtyTracker dirty;
        private readonly PreviewCache cache = new();
        private readonly PreviewRenderer renderer = new();
        private readonly PreviewScheduler scheduler = new();
        private readonly List<NodeId> frameNodes = new();
        private readonly Dictionary<NodeId, Action<Texture>> listeners = new();

        private ShapeGraphAsset asset;
        private int seed = 1337;
        private float timeSeconds;
        private Action<Texture> masterListener;
        private Texture masterTexture;

        public NodePreviewService(GraphDirtyTracker dirtyTracker)
        {
            dirty = dirtyTracker ?? throw new ArgumentNullException(nameof(dirtyTracker));
        }

        public Texture MasterTexture => masterTexture;
        public float TimeSeconds => timeSeconds;

        public void Bind(ShapeGraphAsset graphAsset, int previewSeed, float previewTime = 0f)
        {
            asset = graphAsset;
            seed = previewSeed;
            timeSeconds = previewTime;
            cache.Clear();
            dirty.InvalidateAll(asset);
            scheduler.Hook(Tick);
            RequestMaster();
            if (asset != null)
            {
                for (int i = 0; i < asset.Nodes.Count; i++)
                {
                    if (asset.Nodes[i] != null)
                    {
                        scheduler.Enqueue(asset.Nodes[i].Id);
                    }
                }
            }
        }

        public void SetSeed(int previewSeed)
        {
            if (seed == previewSeed) return;
            seed = previewSeed;
            cache.Clear();
            dirty.InvalidateAll(asset);
            RequestAll();
        }

        public void SetTime(float previewTime)
        {
            if (Mathf.Approximately(timeSeconds, previewTime)) return;
            timeSeconds = previewTime;
            // Time is in the cache key; request master + enqueue visible node rebuilds.
            RequestAll();
        }

        public void Subscribe(NodeId nodeId, Action<Texture> onReady)
        {
            listeners[nodeId] = onReady;
            scheduler.Enqueue(nodeId);
        }

        public void Unsubscribe(NodeId nodeId) => listeners.Remove(nodeId);

        public void SubscribeMaster(Action<Texture> onReady)
        {
            masterListener = onReady;
            RequestMaster();
        }

        public void RequestRebuildAll()
        {
            cache.Clear();
            dirty.InvalidateAll(asset);
            RequestAll();
        }

        public void NotifyDirtyFromTracker()
        {
            var pending = new List<NodeId>();
            dirty.DrainPending(pending);
            for (int i = 0; i < pending.Count; i++)
            {
                cache.InvalidateNode(pending[i]);
                scheduler.Enqueue(pending[i]);
            }

            if (dirty.ConsumeMasterDirty())
                RequestMaster();
        }

        public void Dispose()
        {
            scheduler.Unhook();
            cache.Clear();
            renderer.Dispose();
            listeners.Clear();
            masterListener = null;
            masterTexture = null;
            asset = null;
        }

        private void RequestAll()
        {
            if (asset == null) return;
            for (int i = 0; i < asset.Nodes.Count; i++)
            {
                if (asset.Nodes[i] != null)
                {
                    scheduler.Enqueue(asset.Nodes[i].Id);
                }
            }
            RequestMaster();
        }

        private void RequestMaster() => scheduler.EnqueueMaster();

        private void Tick()
        {
            if (asset == null) return;

            if (dirty.HasPending)
            {
                NotifyDirtyFromTracker();
            }

            scheduler.DrainFrame(frameNodes, out bool includeMaster);

            for (int i = 0; i < frameNodes.Count; i++)
            {
                RebuildNode(frameNodes[i]);
            }

            if (includeMaster)
            {
                RebuildMaster();
            }
        }

        private int TimeQuant => Mathf.RoundToInt(timeSeconds / TimeQuantStep);

        private void RebuildNode(NodeId nodeId)
        {
            ShapeNode node = asset.GetNodeById(nodeId);
            if (node == null) return;

            var key = new PreviewCacheKey(
                nodeId,
                node.Version,
                dirty.StructureVersion,
                seed,
                TimeQuant,
                dirty.PreviewRevision,
                false);

            if (!cache.TryGet(key, out RenderTexture rt))
            {
                PreviewEvalResult result = PreviewEvaluator.EvaluateNode(asset, node, seed, timeSeconds);
                rt = result.Success ? renderer.Render(result.Mesh, 128) : null;

                if (listeners.TryGetValue(nodeId, out Action<Texture> cb))
                {
                    cb?.Invoke(rt);
                }

                if (rt != null)
                {
                    cache.Set(key, rt);
                }
                else
                {
                    cache.ReleaseSlot(nodeId, isMaster: false);
                return;
            }

            if (listeners.TryGetValue(nodeId, out Action<Texture> cachedCb))
            {
                cachedCb?.Invoke(rt);
            }
        }

        private void RebuildMaster()
        {
            var key = new PreviewCacheKey(
                default,
                0,
                dirty.StructureVersion,
                seed,
                TimeQuant,
                dirty.PreviewRevision,
                true);

            if (!cache.TryGet(key, out RenderTexture rt))
            {
                PreviewEvalResult result = PreviewEvaluator.EvaluateMaster(asset, seed, timeSeconds);
                rt = result.Success ? renderer.Render(result.Mesh, 512) : null;

                masterTexture = rt;
                masterListener?.Invoke(rt);

                if (rt != null)
                {
                    cache.Set(key, rt);
                }
                else
                {
                    cache.ReleaseSlot(default, isMaster: true);
                }
                return;
            }

            masterTexture = rt;
            masterListener?.Invoke(rt);
        }
    }
}
