#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Program Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace PiN
{
    /// <summary>
    /// Controls the collision detection and response behavior of a tile.
    /// </summary>
    enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through
        /// it at all. It is completely solid.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but can not fall down through the top of it.
        /// </summary>
        Platform = 2,
    }

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    struct Tile
    {
        public TileCollision Collision;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(X * Width, Y * Height, Width, Height);
            }
        }
        

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(TileCollision collision, int x, int y, int width, int height)
        {
            Collision = collision;
            this.X = x;
            this.Y = y;
            Width = width;
            Height = height;
        }
    }
}
