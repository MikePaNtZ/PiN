using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XnaDebugDrawer;

namespace PiN
{
    class Map
    {
        //Holds the position and properties of an enemy
        public struct InitialEnemy
        {
            public int x;
            public int y;
            public string type;
        }
        

        public InitialEnemy[] Enemies;
        public Graph<Platform> NavMesh;
        public Tile[,] Tiles;
        public List<Platform> Platforms;

        
        private Squared.Tiled.Map map;

        // reordered list of tilesets. ordered by first tile id
        private Dictionary<string, Squared.Tiled.Tileset> tilesets;



        /// <summary>
        /// Width of map measured in tiles.
        /// </summary>
        public int Width { get { return map.Width; } }

        /// <summary>
        /// Height of the map measured in tiles.
        /// </summary>
        public int Height { get { return map.Height; } }

        /// <summary>
        /// Width of tiles in this map.
        /// </summary>
        public int TileWidth { get { return map.TileWidth; } }

        /// <summary>
        /// Height of the tiles in this map.
        /// </summary>
        public int TileHeight { get { return map.TileHeight; } }

        /// <summary>
        /// Returns pixel point of player starting position
        /// </summary>
        public Vector2 StartPoint
        {
            get
            {
                return new Vector2(map.ObjectGroups["events"].Objects["player"].X,
                                 map.ObjectGroups["events"].Objects["player"].Y);
            }
        }

        /// <summary>
        /// Returns tile of player starting position
        /// </summary>
        public Vector2 StartTile
        {
            get
            {
                return new Vector2((int)Math.Floor((float)map.ObjectGroups["events"].Objects["player"].X / map.TileWidth),
                                 (int)Math.Floor((float)map.ObjectGroups["events"].Objects["player"].Y / map.TileHeight));
            }
        }

        /// <summary>
        /// Returns pixel point of exit position
        /// </summary>
        public Point ExitPoint
        {
            get
            {
                return new Point(map.ObjectGroups["events"].Objects["exit"].X,
                                 map.ObjectGroups["events"].Objects["exit"].Y);
            }
        }

        /// <summary>
        /// Returns tile of exit position
        /// </summary>
        public Point ExitTile
        {
            get
            {
                return new Point((int)Math.Floor((float)map.ObjectGroups["events"].Objects["exit"].X / map.TileWidth),
                                 (int)Math.Floor((float)map.ObjectGroups["events"].Objects["exit"].Y / map.TileHeight));
            }
        }

        public Map(string filename, ContentManager content)
        {
            map = Squared.Tiled.Map.Load(filename, content);

            //order tilesets by the first tile id. needed for tile collision
            tilesets = map.Tilesets.OrderBy((item) => item.Value.FirstTileID).ToDictionary(i => i.Key, i => i.Value);
            
            Tiles = new Tile[map.Height,map.Width];
            Enemies = new InitialEnemy[map.ObjectGroups["enemies"].Objects.Count];
            NavMesh = new Graph<Platform>();
            Platforms = new List<Platform>();

            LoadContent();
        }

        private void LoadContent()
        {
            GetTiles();
            GetEnemyInfo();
            CreateNavMesh();
            
        }

        /// <summary>
        /// Loads all tiles into an array
        /// </summary>
        private void GetTiles()
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Tiles[y, x] = new Tile(GetCollision(x, y), x, y, map.TileWidth, map.TileHeight);
                }
            }
        }

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the map boundaries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        private TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= map.Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= map.Height)
                return TileCollision.Passable;

            //get the id of tile
            int tileId = GetTileID(x, y);

            //if tileId is 0 that means there is no tile, so it's passable
            //tileId of -1 means the x or y were off the map
            if (tileId <= 0)
                return TileCollision.Passable;

            Squared.Tiled.Tileset.TilePropertyList currentTileProperties;
            try
            {
                //get list of properties for tile
                currentTileProperties = GetTileProperties(tileId);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Something wrong with getting tileset name " + e);
                return TileCollision.Passable;
            }

            if (currentTileProperties != null) //check if current tile has properties
            {
                switch (Convert.ToInt32(currentTileProperties["TileCollision"]))//should be a number 0-2
                {
                    case 0:
                        return TileCollision.Passable;
                    case 1:
                        return TileCollision.Impassable;
                    case 2:
                        return TileCollision.Platform;
                }
            }

            // changed this to Impassable so that the character won't fall through a tile if its TilCollision property was not set
            //better if its Passable so player can't get stuck, and we'll know something is wrong 
            //because we're falling through tiles that we shouldn't be falling through
            return TileCollision.Passable; //ideally shouldn't actually get to here
        }

        /// <summary>
        /// Returns the tile id of the tile at the x and y coordinates. The x and y are tile based not pixel based
        /// </summary>
        private int GetTileID(int x, int y)
        {
            try
            {
                return map.Layers["Foreground"].GetTile(x, y);
            }
            catch (InvalidOperationException e)
            {
                System.Diagnostics.Debug.WriteLine("X or Y value out of range of map\n" + e);
                return -1;
            }
        }

        /// <summary>
        /// Returns the property list of the current tile id. First it has to find which tileset the tile belongs to.
        /// The tilesets are ordered by the FirstTileID. This is done by looping through the tilesets and comparing the first tile id to the current tile id. 
        /// If it is greater then return the previous tileset properties.
        /// </summary>
        private Squared.Tiled.Tileset.TilePropertyList GetTileProperties(int tileId)
        {
            //If there is only one tileset return its properties
            if (tilesets.Count == 1)
                return tilesets.First().Value.GetTileProperties(tileId);

            //loops through all the tilesets
            for (int i = 1; i < tilesets.Count; i++)//start at the second one
            {
                //checks if first tile id of the tileset is greater than current tile id
                //If it is then return the previous tileset properties
                if (tilesets.ElementAt(i).Value.FirstTileID > tileId)
                    return tilesets.ElementAt(i - 1).Value.GetTileProperties(tileId);
            }

            //if tileId is greater than all of the FirstTileID of the tilesets then it has to be the last one
            return tilesets.Last().Value.GetTileProperties(tileId);
        }

        /// <summary>
        /// Gets the points and properties of enemies in the map. Stores in a public list enemies
        /// </summary>
        private void GetEnemyInfo()
        {
            //first is named enemy without a number after it
            Enemies[0].x = map.ObjectGroups["enemies"].Objects["enemy"].X;
            Enemies[0].y = map.ObjectGroups["enemies"].Objects["enemy"].Y;
            Enemies[0].type = map.ObjectGroups["enemies"].Objects["enemy"].Properties["enemyType"];

            //the rest are called enemy1, enemy2, etc.
            for (int i = 1; i < map.ObjectGroups["enemies"].Objects.Values.Count((item) => item.Name.Equals("enemy")); i++)
            {
                Enemies[i].x = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].X;
                Enemies[i].y = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].Y;
                Enemies[i].type = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].Properties["enemyType"];
            }
        }

        /// <summary>
        /// Creates Navigation mesh used for ai
        /// </summary>
        private void CreateNavMesh()
        {
            
            TileCollision currentTileCollision;
            TileCollision above, below;
            int platformStart = -1;
            Platform tempPlatform;
            

            //This steps through each tile looking for valid walking platforms. A valid walking platforms is two passable tiles on an impassable/platform tile
            //Once it finds one it stores the edge in a platform object and adds it as a node
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    currentTileCollision = Tiles[y,x].Collision;
                    if (y - 1 >= 0)
                        above = Tiles[y - 1,x].Collision;
                    else
                        above = TileCollision.Passable;

                    if (y + 1 < Height)
                        below = Tiles[y + 1,x].Collision;
                    else
                        below = TileCollision.Passable;

                    //if current tile is valid for walking on. two passable on top of impassable/platform
                    if ((currentTileCollision == TileCollision.Passable || currentTileCollision == TileCollision.Platform) &&
                        (above == TileCollision.Passable || above == TileCollision.Platform) &&
                        (below == TileCollision.Impassable || below == TileCollision.Platform))
                    {
                        if (platformStart == -1)//left edge isn't set yet means new platform!
                            platformStart = x;

                        if (x == Width - 1)//at the end of the map? end the platform
                        {
                            tempPlatform = new Platform(platformStart * TileWidth, (x * TileWidth) + TileWidth, (y * TileHeight) + TileHeight);
                            Platforms.Add(tempPlatform);
                            NavMesh.AddNode(tempPlatform);
                            platformStart = -1;
                        }
                    }
                    else if (platformStart != -1)//end of current platform
                    {
                        tempPlatform = new Platform(platformStart*TileWidth, x*TileWidth, (y*TileHeight)+TileHeight);
                        Platforms.Add(tempPlatform);
                        NavMesh.AddNode(tempPlatform);
                        platformStart = -1;
                    }
                    
                }
            }

            ConnectNodes();
        }

        /// <summary>
        /// Finds any nodes that could connect to current node, and adds them to current node
        /// </summary>
        private void ConnectNodes()
        {
            foreach (GraphNode<Platform> gNode in NavMesh)
            {
                AddEdges(gNode);
            }
            
        }

        private void AddEdges(GraphNode<Platform> gNode)
        {
            int top = (int)Math.Floor((float)gNode.Value.Y / TileHeight) - 4;
            int bottom = (int)Math.Floor((float)gNode.Value.Y / TileHeight) + 14;
            int left = (int)Math.Floor((float)gNode.Value.LeftEdgeX / TileWidth) - 4;
            int right = (int)Math.Floor((float)gNode.Value.RightEdgeX / TileWidth) + 4;

            if (top < 0)
                top = 0;
            if (bottom > Height)
                bottom = Height;
            if (left < 0)
                left = 0;
            if (right > Width)
                right = Width;

            GraphNode<Platform> fromNode = (GraphNode<Platform>)NavMesh.Nodes.FindByValue(gNode.Value);
            GraphNode<Platform> toNode;
            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    toNode = (GraphNode<Platform>)NavMesh.Nodes.FirstOrDefault(node => node.Value.Y == y*TileHeight && node.Value.LeftEdgeX <= x*TileWidth && node.Value.RightEdgeX >= x*TileWidth);
                    if (toNode != null && !toNode.Equals(fromNode) && !fromNode.Neighbors.Contains(toNode))
                        NavMesh.AddDirectedEdge(fromNode, toNode, Math.Abs(fromNode.Value.Y - toNode.Value.Y));
                }
            }

            //GraphNode<Tile> tempGNode;
            //foreach (var node in possibleNeighbors)
            //{
            //    if (node.Value.X > left)
            //    {
            //        tempTile = new Tile(GetCollision(node.Value.X-1, node.Value.Y),node.Value.X-1,node.Value.Y,TileWidth,TileHeight);
            //        tempGNode = (GraphNode<Tile>)possibleNeighbors.Nodes.FindByValue(tempTile);
            //        if (tempGNode != null)
            //            possibleNeighbors.AddDirectedEdge(node,tempGNode,1);
            //    }
            //    if (node.Value.X < right)
            //    {
            //        tempTile = new Tile(GetCollision(node.Value.X + 1, node.Value.Y), node.Value.X + 1, node.Value.Y, TileWidth, TileHeight);
            //        tempGNode = (GraphNode<Tile>)possibleNeighbors.Nodes.FindByValue(tempTile);
            //        if (tempGNode != null)
            //            possibleNeighbors.AddDirectedEdge(node, tempGNode, 1);
            //    }
            //    if (node.Value.Y > top)
            //    {
            //        tempTile = new Tile(GetCollision(node.Value.X, node.Value.Y - 1), node.Value.X, node.Value.Y - 1, TileWidth, TileHeight);
            //        tempGNode = (GraphNode<Tile>)possibleNeighbors.Nodes.FindByValue(tempTile);
            //        if (tempGNode != null)
            //            possibleNeighbors.AddDirectedEdge(node, tempGNode, 1);
            //    }
            //    if (node.Value.Y < bottom)
            //    {
            //        tempTile = new Tile(GetCollision(node.Value.X, node.Value.Y+1), node.Value.X, node.Value.Y+1, TileWidth, TileHeight);
            //        tempGNode = (GraphNode<Tile>)possibleNeighbors.Nodes.FindByValue(tempTile);
            //        if (tempGNode != null)
            //            possibleNeighbors.AddDirectedEdge(node, tempGNode, 1);
            //    }
            //}
            //return possibleNeighbors;
        }

        //private void AddEdgesToNavMesh(Graph<Tile> possibleNeighbors, GraphNode<Tile> root)
        //{
        //    Queue<GraphNode<Tile>> frontier = new Queue<GraphNode<Tile>>();
        //    NodeList<Tile> visited = new NodeList<Tile>();
        //    GraphNode<Tile> current, toNode, fromNode;
        //    Tile tempTile;

        //    fromNode = (GraphNode<Tile>)NavMesh.Nodes.FindByValue(root.Value);

        //    frontier.Enqueue(root);
        //    visited.Add(root);
        //    while (frontier.Count > 0)
        //    {
        //        current = frontier.Dequeue();

        //        toNode = (GraphNode<Tile>)NavMesh.Nodes.FindByValue(current.Value);


        //        if (toNode != null && !toNode.Equals(fromNode))
        //        {
        //            NavMesh.AddDirectedEdge(fromNode, toNode, 1);
        //            //if (fromNode.Neighbors.Count != 0)
        //            //{
        //            //    foreach (GraphNode<Tile> neighbor in fromNode.Neighbors)
        //            //    {
        //            //        if (toNode.Value.x < fromNode.Value.x && neighbor.Value.x < fromNode.Value.x)
        //            //        {
        //            //            if (neighbor.Value.x > toNode.Value.x)
        //            //                NavMesh.AddDirectedEdge(fromNode, toNode, 1);
        //            //        }

        //            //        else if (toNode.Value.x > fromNode.Value.x && !(neighbor.Value.x < toNode.Value.x))
        //            //            NavMesh.AddDirectedEdge(fromNode, toNode, 1);
        //            //    }
        //            //}
        //            //else
        //            //    NavMesh.AddDirectedEdge(fromNode, toNode, 1);


        //        }


        //        foreach (GraphNode<Tile> neighbor in current.Neighbors)
        //        {
        //            if (visited.FindByValue(neighbor.Value) == null)
        //            {
        //                visited.Add(neighbor);
        //                frontier.Enqueue(neighbor);
        //            }


        //        }
        //    }
        //}

        /// <summary>
        /// Draws the entire map
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Camera cam, bool drawNavMesh)
        {
            map.Draw(spriteBatch, new Rectangle((int)cam.Position.X, (int)cam.Position.Y, (int)(cam.ViewPort.Width), (int)(cam.ViewPort.Height)), cam.Position);
            //map.Draw(spriteBatch, cam.ViewPort.Bounds, cam.Position);

            if (drawNavMesh)
            {
                Vector2 left, right;
                Vector2 neighborLeft, neighborRight;
                Color lineColor;
                foreach (GraphNode<Platform> gNode in NavMesh)
                {
                    left = new Vector2(gNode.Value.LeftEdgeX, gNode.Value.Y);
                    right = new Vector2(gNode.Value.RightEdgeX, gNode.Value.Y);
                    spriteBatch.DrawLineSegment(left, right, Color.Blue, 5);
                    spriteBatch.DrawCircle(left, 5, Color.Blue, 5);
                    spriteBatch.DrawCircle(right, 5, Color.Blue, 5);

                    foreach (GraphNode<Platform> neighbor in gNode.Neighbors)
                    {
                        neighborLeft = new Vector2(neighbor.Value.LeftEdgeX, neighbor.Value.Y);
                        neighborRight = new Vector2(neighbor.Value.RightEdgeX, neighbor.Value.Y);


                        if (neighbor.Value.Y > gNode.Value.Y)
                            lineColor = Color.Orange;
                        else
                            lineColor = Color.Aqua;

                        //spriteBatch.DrawLineSegment(left, neighborLeft, Color.Aqua, 2);
                        if (neighborRight.X <= left.X)
                            spriteBatch.DrawLineSegment(left, neighborRight, lineColor, 2);
                        else if (neighborRight.X >= left.X && neighborRight.X <= right.X && neighborLeft.X <= left.X)
                            spriteBatch.DrawLineSegment(left, new Vector2((neighborRight.X + neighborLeft.X) / 2, neighborLeft.Y), lineColor, 2);
                        else if (neighborRight.X >= right.X && neighborLeft.X <= right.X && neighborLeft.X >= left.X)
                            spriteBatch.DrawLineSegment(right, new Vector2((neighborRight.X + neighborLeft.X) / 2, neighborLeft.Y), lineColor, 2);
                        else if (neighborLeft.X >= right.X)
                            spriteBatch.DrawLineSegment(right, neighborLeft, lineColor, 2);
                        else
                            spriteBatch.DrawLineSegment(left, neighborLeft, lineColor, 2);
                    }

                }
            }
            
        }

    }
}
