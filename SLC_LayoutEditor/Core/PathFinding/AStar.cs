using System;
using System.Collections.Generic;

namespace SLC_LayoutEditor.Core.PathFinding
{
    internal class AStar
    {
        public static List<Node> FindPath(Node[,] grid, Node startNode, Node targetNode)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].F < currentNode.F || (openSet[i].F == currentNode.F && openSet[i].H < currentNode.H))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return GetPath(startNode, targetNode);
                }

                foreach (Node neighbor in GetNeighbors(grid, currentNode))
                {
                    if (neighbor == null || closedSet.Contains(neighbor) || 
                        neighbor.IsObstacle && (neighbor.IsObstacleOverride == null || neighbor.IsObstacleOverride.Value))
                    {
                        continue;
                    }

                    int newG = currentNode.G + 1; // Assuming a simple cost of 1 for each movement

                    if (!openSet.Contains(neighbor) || newG < neighbor.G)
                    {
                        neighbor.G = newG;
                        neighbor.H = CalculateHCost(neighbor, targetNode);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        private static List<Node> GetPath(Node startNode, Node targetNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = targetNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private static List<Node> GetNeighbors(Node[,] grid, Node node)
        {
            List<Node> neighbors = new List<Node>();
            int maxX = grid.GetLength(0);
            int maxY = grid.GetLength(1);

            if (node.X > 0) // Left
                neighbors.Add(grid[node.X - 1, node.Y]);
            if (node.X < maxX - 1) // Right
                neighbors.Add(grid[node.X + 1, node.Y]);
            if (node.Y > 0) // Up
                neighbors.Add(grid[node.X, node.Y - 1]);
            if (node.Y < maxY - 1) // Down
                neighbors.Add(grid[node.X, node.Y + 1]);

            return neighbors;
        }

        private static int CalculateHCost(Node from, Node to)
        {
            // Using Manhattan distance as the heuristic
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
    }
}
