#region File Description
//-----------------------------------------------------------------------------
// Platform.cs
//
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace PiN
{

    /// <summary>
    /// Stores the position of a platform on the map. Used for ai navigation.
    /// </summary>
    struct Platform
    {
        public int LeftEdgeX;
        public int RightEdgeX;
        public int Y;
        public int Width;


        /// <summary>
        /// Constructs a new platform.
        /// </summary>
        public Platform(int leftX, int rightX, int y)
        {
            this.LeftEdgeX = leftX;
            this.RightEdgeX = rightX;
            this.Y = y;
            this.Width = rightX - leftX;
        }
    }
}
