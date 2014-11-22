using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PiN
{
    class Collision
    {
        private static Level level;

        public static void LoadLevel(Level lvl)
        {
            level = lvl;
        }

        public static float? RayCastCollidesWithLevel(Vector2 source, Vector2 destination)
        {
            BoundingBox tempBounds;
            Vector3 direction = new Vector3(destination - source, 0);
            direction.Normalize();
            Ray ray = new Ray(new Vector3(source,0), direction);
            float? intersectDistance;

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    if (level.Tiles[y, x].Collision != TileCollision.Passable)
                    {
                        tempBounds = new BoundingBox(new Vector3(level.Tiles[y, x].Rectangle.X, level.Tiles[y, x].Rectangle.Y + level.TileHeight, 0),
                                                 new Vector3(level.Tiles[y, x].Rectangle.X + level.TileWidth, level.Tiles[y, x].Rectangle.Y, 0));

                        intersectDistance = ray.Intersects(tempBounds);
                        if (intersectDistance != null && intersectDistance * intersectDistance <= (destination - source).LengthSquared())
                            return intersectDistance;
                    }
                    else
                        intersectDistance = null;
                    
                }
            }
            return null;
        }
    }
}
