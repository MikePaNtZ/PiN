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

            Graph<Platform> path = new Graph<Platform>();




            return path;
        }
    }
}
