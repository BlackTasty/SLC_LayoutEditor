using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.PathFinding
{
    internal class Dijkstra
    {
        public static List<Node> FindPath(Node[,] grid, Node startNode, Node targetNode)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.Cost = 0;
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = GetNodeWithLowestCost(openSet);

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return GetPath(startNode, targetNode);
                }

                foreach (Node neighbor in GetNeighbors(grid, currentNode))
                {
                    if (closedSet.Contains(neighbor) || neighbor.IsObstacle)
                    {
                        continue;
                    }

                    int newCost = currentNode.Cost + 1; // Assuming a simple cost of 1 for each movement

                    if (newCost < neighbor.Cost)
                    {
                        neighbor.Cost = newCost;
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

        private static Node GetNodeWithLowestCost(List<Node> nodes)
        {
            Node lowestCostNode = nodes[0];
            foreach (Node node in nodes)
            {
                if (node.Cost < lowestCostNode.Cost)
                {
                    lowestCostNode = node;
                }
            }
            return lowestCostNode;
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
    }
}
