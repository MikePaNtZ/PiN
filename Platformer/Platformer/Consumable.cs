#region File Description
//-----------------------------------------------------------------------------
// Gem.cs
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
    /// defines consumable type
    /// </summary>
    enum ConsumableType
    {
        /// <summary>
        /// Increases player health
        /// </summary>
        Health = 0,

        /// <summary>
        /// Powers up player
        /// </summary>
        PowerUp = 1,
    }

    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    class Consumable
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly ConsumableType consumableType;
        public readonly Color Color;

        // The Consumable is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

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
        public Consumable(Level level, Vector2 position, ConsumableType type)
        {
            this.level = level;
            this.basePosition = position;
            consumableType = type;

            switch (consumableType)
            {
                case ConsumableType.Health:
                    Color = Color.White;
                    break;
                case ConsumableType.PowerUp:
                    Color = Color.Red;
                    break;
            }
    
            LoadContent();
        }

        /// <summary>
        /// Loads the consumable texture and collected sound.
        /// </summary>
        public void LoadContent()
        {

            switch (consumableType)
            {
                case ConsumableType.Health:
                    texture = Level.Content.Load<Texture2D>("Sprites/Gem");
                    break;
                case ConsumableType.PowerUp:
                    texture = Level.Content.Load<Texture2D>("Sprites/Gem");
                    break;
            }
            //texture = Level.Content.Load<Texture2D>("Sprites/Gem");
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
        /// The player who collected this gem. Although currently not used, this parameter would be
        /// useful for creating special powerup gems. For example, a gem could make the player invincible.
        /// </param>
        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
            switch (consumableType)
            {
                case ConsumableType.Health:
                    collectedBy.UpdateHealth(15);
                    break;
                case ConsumableType.PowerUp:
                    collectedBy.PowerUp();
                    break;
            }
    
        }

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            //spriteBatch.End();
        }
    }
}
