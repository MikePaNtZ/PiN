using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class SearchState : BehaviorState
    {
        public SearchState(EnemyStateMachine ESM)
            : base(ESM)
        {
            enemy.Color = Color.White;
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Calculate tile position based on the side we are walking towards.
            float posX = enemy.Position.X + enemy.BoundingRectangle.Width / 2 * (int)enemy.FaceDirection;
            int tileX = (int)Math.Floor(posX / enemy.Level.TileWidth) - (int)enemy.FaceDirection;
            int tileY = (int)Math.Floor(enemy.Position.Y / enemy.Level.TileHeight);

            if (enemy.WaitTime > 0)
            {
                // Wait for some amount of time.
                enemy.WaitTime = Math.Max(0.0f, enemy.WaitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (enemy.WaitTime <= 0.0f)
                {
                    // Then turn around.
                    enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY - 1) == TileCollision.Impassable)
                {
                    enemy.WaitTime = enemy.MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    enemy.Move(enemy.FaceDirection);
                }
            }

            base.Update(gameTime, gameInputs);

            if (enemy.LineOfSight.X * (int)enemy.FaceDirection >= 0) //make sure enemy is facing the right direction
            {
                if (Math.Abs(enemy.LineOfSight.X) <= enemy.MaxAttackDistance)// player is in attacking distance then attack
                    esm.BehaviorState = new AttackState(esm);
                else if (Math.Abs(enemy.LineOfSight.X) <= enemy.MinTrackDistance)//or at least in tracking distance then track
                    esm.BehaviorState = new TrackState(esm);
            }
        }
    }
}
