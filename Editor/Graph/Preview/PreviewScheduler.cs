using System;
using System.Collections.Generic;
using Galaretka.ShapeGraph.Core;
using UnityEditor;

namespace Galaretka.ShapeGraph.Editor.Graph.Preview
{
    /// <summary>
    /// Budgets preview rebuilds across editor frames.
    /// </summary>
    public sealed class PreviewScheduler
    {
        private readonly Queue<NodeId> queue = new();
        private readonly HashSet<NodeId> enqueued = new();
        private bool masterQueued;
        private bool hooked;
        private Action tick;

        public int MaxPerFrame { get; set; } = 4;

        public void Hook(Action onTick)
        {
            tick = onTick;
            if (hooked)
                return;
            EditorApplication.update += OnEditorUpdate;
            hooked = true;
        }

        public void Unhook()
        {
            if (!hooked)
                return;
            EditorApplication.update -= OnEditorUpdate;
            hooked = false;
            tick = null;
            queue.Clear();
            enqueued.Clear();
            masterQueued = false;
        }

        public void Enqueue(NodeId nodeId)
        {
            if (!nodeId.IsValid || !enqueued.Add(nodeId))
                return;
            queue.Enqueue(nodeId);
        }

        public void EnqueueMaster() => masterQueued = true;

        public void DrainFrame(List<NodeId> nodesOut, out bool includeMaster)
        {
            nodesOut.Clear();
            includeMaster = false;

            int budget = MaxPerFrame;
            if (masterQueued)
            {
                includeMaster = true;
                masterQueued = false;
                budget = Math.Max(0, budget - 1);
            }

            while (budget > 0 && queue.Count > 0)
            {
                NodeId id = queue.Dequeue();
                enqueued.Remove(id);
                nodesOut.Add(id);
                budget--;
            }
        }

        private void OnEditorUpdate() => tick?.Invoke();
    }
}
