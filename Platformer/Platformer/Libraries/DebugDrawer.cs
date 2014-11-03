﻿#region File Description
//-----------------------------------------------------------------------------
// DebugDrawer.cs
//
// Visit Bayinx.wordpress. for more tutorials ;).
//
// Copyright Bayinx. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaDebugDrawer
{
    public static class DebugDrawer
    {
        private static Texture2D _blankTexture;

        public static void LoadContent(GraphicsDevice graphicsDevice)
        {
            _blankTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _blankTexture.SetData(new[] { Color.White });
        }

        public static void LoadContent(Texture2D blankTexture)
        {
            _blankTexture = blankTexture;
        }

        public static void DrawLineSegment(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, int lineWidth)
        {

            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(_blankTexture, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, lineWidth),
                       SpriteEffects.None, 0);
        }

        public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2[] vertex, int count, Color color, int lineWidth)
        {
            if (count > 0)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    DrawLineSegment(spriteBatch, vertex[i], vertex[i + 1], color, lineWidth);
                }
                DrawLineSegment(spriteBatch, vertex[count - 1], vertex[0], color, lineWidth);
            }
        }

        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, int lineWidth)
        {
            Vector2[] vertex = new Vector2[4];
            vertex[0] = new Vector2(rectangle.Left, rectangle.Top);
            vertex[1] = new Vector2(rectangle.Right, rectangle.Top);
            vertex[2] = new Vector2(rectangle.Right, rectangle.Bottom);
            vertex[3] = new Vector2(rectangle.Left, rectangle.Bottom);

            DrawPolygon(spriteBatch, vertex, 4, color, lineWidth);
        }

        public static void DrawCircle(this SpriteBatch spritbatch, Vector2 center, float radius, Color color, int lineWidth, int segments = 16)
        {

            Vector2[] vertex = new Vector2[segments];

            double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            for (int i = 0; i < segments; i++)
            {
                vertex[i] = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                theta += increment;
            }

            DrawPolygon(spritbatch, vertex, segments, color, lineWidth);
        }
    }
}

