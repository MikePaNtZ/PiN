#region File Description
//-----------------------------------------------------------------------------
// Consumable.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer
{

    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    abstract class Consumable
    {
        protected Texture2D texture;
        protected Vector2 origin;
        protected SoundEffect collectedSound;
        protected Color color;

        // The Consumable is animated from a base position along the Y axis.
        protected Vector2 basePosition;
        protected float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Gets the current position of this consumable in world space.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this consumable in world space.
        /// </summary>
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, level.TileWidth / 3.0f);
            }
        }

        /// <summary>
        /// Constructs a new consumable.
        /// </summary>
        public Consumable(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;
        }

        /// <summary>
        /// Loads the consumable texture and collected sound.
        /// </summary>
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring consumable bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        /// <summary>
        /// Called when this item has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this gem.
        /// </param>
        abstract public void OnCollected(Hero collectedBy);

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, null, color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            //spriteBatch.End();
        }
    }
}
