using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
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
        

        public List<InitialEnemy> Enemies;
        public Graph<Tile> NavMesh;

        
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

            CreateNavMesh();
            GetEnemyInfo();
        }

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the map boundaries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
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
            if (tileId == 0 || tileId == -1)
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
            InitialEnemy enemy;
            Enemies = new List<InitialEnemy>();

            //first is named enemy without a number after it
            enemy.x = map.ObjectGroups["enemies"].Objects["enemy"].X;
            enemy.y = map.ObjectGroups["enemies"].Objects["enemy"].Y;
            enemy.type = map.ObjectGroups["enemies"].Objects["enemy"].Properties["enemyType"];

            Enemies.Add(enemy);

            //the rest are called enemy1, enemy2, etc.
            for (int i = 1; i < map.ObjectGroups["enemies"].Objects.Values.Count((item) => item.Name.Equals("enemy")); i++)
            {
                enemy.x = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].X;
                enemy.y = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].Y;
                enemy.type = map.ObjectGroups["enemies"].Objects[String.Format("enemy{0}", i)].Properties["enemyType"];

                Enemies.Add(enemy);
            }
        }

        /// <summary>
        /// Creates Navigation mesh used for ai
        /// </summary>
        private void CreateNavMesh()
        {

        }

        /// <summary>
        /// Draws the entire map
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Camera cam)
        {
            map.Draw(spriteBatch, new Rectangle((int)cam.Position.X, (int)cam.Position.Y, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), cam.Position);
        }
    }
}
