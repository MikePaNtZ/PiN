
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Platformer
{
    class PhysicsEngine
    {

        public PhysicsEngine(GameCharacter gameChar)
        {
            character = gameChar;
        }

        /// <summary>
        /// Updates a game character's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = character.Position;
            Vector2 characterVelocity = character.Velocity;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            characterVelocity.X += character.Movement * MoveAcceleration * elapsed;
            characterVelocity.Y = MathHelper.Clamp(characterVelocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            characterVelocity.Y = DoJump(characterVelocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (character.IsOnGround)
                characterVelocity.X *= GroundDragFactor;
            else
                characterVelocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            characterVelocity.X = MathHelper.Clamp(characterVelocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply Velocity.
            character.Position += characterVelocity * elapsed;
            character.Position = new Vector2((float)Math.Round(character.Position.X), (float)Math.Round(character.Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the Velocity to zero.
            if (character.Position.X == previousPosition.X)
                characterVelocity.X = 0;

            if (character.Position.Y == previousPosition.Y)
            {
                characterVelocity.Y = 0;
                jumpTime = 0.0f;
            }
            character.Velocity = characterVelocity;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (character.IsJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && character.IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        character.JumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    character.Sprite.LoadAnimation(character.JumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = character.IsJumping;
            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = character.BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / character.Level.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / character.Level.TileWidth)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / character.Level.TileHeight);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / character.Level.TileHeight)) - 1;

            // Reset flag to search for ground collision.
            character.IsOnGround = false;

            // TODO This shit is overly complex. We should clean it up if we get time.
            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = character.Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = character.Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    character.IsOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || character.IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    character.Position = new Vector2(character.Position.X, character.Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = character.BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                character.Position = new Vector2(character.Position.X + depth.X, character.Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = character.BoundingRectangle;
                            }
                        }
                    }
                }
            }
            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        // Constants for controling horizontal movement. Can be overriden
        protected const float MoveAcceleration = 13000.0f;
        protected const float MaxMoveSpeed = 1750.0f;
        protected const float GroundDragFactor = 0.48f;
        protected const float AirDragFactor = 0.58f;
        // Constants for controlling vertical movement. Can be overriden. Could also be made a property of the game character class for
        // customization between game characters.
        protected const float MaxJumpTime = 0.35f;
        protected const float JumpLaunchVelocity = -3500.0f;
        protected const float GravityAcceleration = 3400.0f;
        protected const float MaxFallSpeed = 550.0f;
        protected const float JumpControlPower = 0.14f;
        // The character that this physics engine is controlling
        private GameCharacter character;
        // Bottom bounds for the character
        private float previousBottom;
        // Flag indicating that the game character was jumping in the previous call to apply the physics.
        private bool wasJumping;
        // Jump time state variable used by the physics engine for character jumping.
        private float jumpTime;

    }
}