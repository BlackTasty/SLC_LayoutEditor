using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Tasty.Logging;

namespace SLC_LayoutEditor.Core.PathFinding
{
    internal class CabinPathGrid
    {
        private readonly CabinDeck cabinDeck;
        private List<Node> overriddenNodes = new List<Node>();

        private Node[,] grid;
        private int width;
        private int height;

        private Node startNode;
        private Node endNode;

        public CabinPathGrid(CabinDeck cabinDeck)
        {
            this.cabinDeck = cabinDeck;
            UpdateMap();
        }

        public void UpdateMap()
        {
            Logger.Default.WriteLog("Updating path map for cabin deck \"{0}\"", cabinDeck.FloorName);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            width = cabinDeck.Rows + 1;
            height = cabinDeck.Columns + 1;
            grid = new Node[width, height];

            foreach (CabinSlot slot in cabinDeck.CabinSlots)
            {
                grid[slot.Row, slot.Column] = new Node(slot);
            }
            sw.Stop();
            Logger.Default.WriteLog("Path map update complete, took {0} seconds", sw.GetElapsedSecondsForLog());

            //PrintPathMap();
        }

        public bool HasPathToAny(CabinSlot start, IEnumerable<CabinSlot> ends)
        {
            foreach (CabinSlot end in ends)
            {
                if (HasPathTo(start, end))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPathTo(CabinSlot start, CabinSlot end)
        {
            startNode = grid[start.Row, start.Column];
            endNode = grid[end.Row, end.Column];

            OverrideAllowedNodes();
            List<Node> path = AStar.FindPath(grid, startNode, endNode);

            overriddenNodes.ForEach(x => x.IsObstacleOverride = null);
            overriddenNodes.Clear();

            return path != null;
        }

        private void OverrideAllowedNodes()
        {
            Node seatNode = startNode.Slot.IsSeat ? startNode : endNode.Slot.IsSeat ? endNode : null;

            if (seatNode != null)
            {
                CabinSlot nearestAisle = cabinDeck.GetNearestServiceArea(seatNode.Slot);

                if (nearestAisle != null)
                {
                    int startColumn = seatNode.Slot.Column < nearestAisle.Column ? seatNode.Slot.Column : nearestAisle.Column;
                    int endColumn = seatNode.Slot.Column > nearestAisle.Column ? seatNode.Slot.Column : nearestAisle.Column;
                    int row = nearestAisle.Row;

                    for (int col = startColumn; col < endColumn; col++)
                    {
                        grid[row, col].IsObstacleOverride = false;
                        overriddenNodes.Add(grid[row, col]);
                    }
                }
            }
        }

        private void PrintPathMap()
        {
            #region Print path map to console
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(grid[x, y].IsObstacle && (grid[x, y].IsObstacleOverride == null || grid[x, y].IsObstacleOverride.Value) ? " 0 " : " 1 ");
                    if (x < width - 1)
                    {
                        sb.Append(",");
                    }
                }
                if (y < height - 1)
                {
                    sb.Append("\n");
                }
            }

            Console.WriteLine(sb.ToString());
            #endregion
        }
    }
}
