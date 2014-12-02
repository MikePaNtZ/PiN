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

        static int top;
        static int bottom;
        static int left;
        static int right;

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

            

            top = (int)Math.Floor((float)Math.Min(destination.Y,source.Y) / level.TileHeight) - 1;
            bottom = (int)Math.Floor((float)Math.Max(destination.Y,source.Y) / level.TileHeight) + 1;
            left = (int)Math.Floor((float)Math.Min(destination.X,source.X) / level.TileWidth) - 1;
            right = (int)Math.Floor((float)Math.Max(destination.X,source.X) / level.TileWidth) + 1;

            if (top < 0)
                top = 0;
            if (bottom > level.Height)
                bottom = level.Height;
            if (left < 0)
                left = 0;
            if (right > level.Width)
                right = level.Width;

            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
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

        public static void Draw(SpriteBatch spriteBatch)
        {
            Rectangle bounds = new Rectangle(left * level.TileWidth, top * level.TileHeight, (right - left) * level.TileWidth, (bottom - top) * level.TileHeight);
            XnaDebugDrawer.DebugDrawer.DrawRectangle(spriteBatch, bounds, Color.Red, 2);
        }
    }
}
