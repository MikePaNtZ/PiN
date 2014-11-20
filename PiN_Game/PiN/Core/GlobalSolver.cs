using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PiN
{
    class GlobalSolver
    {
        static private Graph<Platform> navmesh;

        static public void LoadMesh(Graph<Platform> mesh)
        {
            navmesh = mesh;
        }

        static public Graph<Platform> FindPath(Platform start, Platform finish)
        {
            if (navmesh == null)
                throw new NullReferenceException("No Navigation Mesh Loaded.");

            GraphNode<Platform> startNode = (GraphNode<Platform>)navmesh.Nodes.FindByValue(start);
            GraphNode<Platform> finishNode = (GraphNode<Platform>)navmesh.Nodes.FindByValue(finish);

            if (startNode == null || finishNode == null)
                throw new ArgumentNullException("Start or finish platform is not in nav mesh");

            Graph<Platform> path = new Graph<Platform>();

            Queue<GraphNode<Platform>> frontier = new Queue<GraphNode<Platform>>();
            Dictionary<GraphNode<Platform>, GraphNode<Platform>> cameFrom = new Dictionary<GraphNode<Platform>,GraphNode<Platform>>();
            GraphNode<Platform> current;

            

            frontier.Enqueue(startNode);
            cameFrom.Add(startNode,null);
            bool foundIt = false;
            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();


                if (current.Equals(finishNode))
                {
                    foundIt = true;
                    break;
                }
                    


                foreach (GraphNode<Platform> neighbor in current.Neighbors)
                {
                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        cameFrom.Add(neighbor,current);
                        frontier.Enqueue(neighbor);
                    }
                }
            }

            if (foundIt == false)
                return null;

            GraphNode<Platform> previous;
            current = finishNode;
            path.AddNode(current.Value);
            while (current != startNode)
            {
                previous = current;
                current = cameFrom[current];
                path.AddNode(current.Value);
                path.AddDirectedEdge((GraphNode<Platform>)path.Nodes.FindByValue(current.Value), (GraphNode<Platform>)path.Nodes.FindByValue(previous.Value),1);
            }

            return path;
        }
    }
}
