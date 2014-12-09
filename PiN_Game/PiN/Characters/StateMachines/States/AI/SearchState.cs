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
            esm.ShooterState = new EnemyAimingState(esm);
            enemy.WaitTime = 0.0f;
            
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (enemy.Health <= enemy.MaxHealth * enemy.KamikazeThresholdPercent)
            {
                esm.BehaviorState = new KamikazeState(esm);
                return;
            }

            

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            enemy.Target = new Vector2(enemy.Center.X + ((int)enemy.FaceDirection * enemy.MinTrackDistance), enemy.Center.Y - ((int)enemy.FaceDirection * enemy.WaitTime * 100));
            

            // Calculate tile position based on the side we are walking towards.
            float posX = enemy.Position.X;// +enemy.BoundingRectangle.Width / 2 * (int)enemy.FaceDirection;
            int tileX = (int)Math.Floor(posX / enemy.Level.TileWidth);// -(int)enemy.FaceDirection;
            int tileY = (int)Math.Floor(enemy.Position.Y / enemy.Level.TileHeight);

            if (enemy.WaitTime > 0)
            {
                // Wait for some amount of time.
                enemy.WaitTime = Math.Max(0.0f, enemy.WaitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (enemy.WaitTime <= 0f)
                {
                    // Then turn around.
                    enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection);
                    enemy.Target = new Vector2(enemy.Center.X + ((int)enemy.FaceDirection * enemy.MinTrackDistance), enemy.Center.Y - ((int)enemy.FaceDirection * enemy.WaitTime * 100));
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY - 1) == TileCollision.Impassable ||
                    enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY) == TileCollision.Passable)
                {
                    enemy.WaitTime = enemy.MaxWaitTime;
                }
                else
                {
                    enemy.Move(enemy.FaceDirection);
                }
            }

            

            if (enemy.LineOfSightToHero.X * (int)enemy.FaceDirection >= 0) //make sure enemy is facing the right direction
            {
                if (Math.Abs(enemy.LineOfSightToHero.X) <= enemy.MaxAttackDistance &&
                    Math.Abs(enemy.LineOfSightToHero.Y) <= enemy.MaxAttackDistance)// player is in attacking distance then attack
                {
                    if (Collision.RayCastCollidesWithLevel(enemy.Center, enemy.Level.ActiveHero.Center) == null)
                    {
                        enemy.WaitTime = 0;
                        esm.BehaviorState = new AttackState(esm);
                    }
                        
                }
                else if (Math.Abs(enemy.LineOfSightToHero.X) <= enemy.MinTrackDistance &&
                         Math.Abs(enemy.LineOfSightToHero.Y) <= enemy.MinTrackDistance)//or at least in tracking distance then track
                {
                    if (Collision.RayCastCollidesWithLevel(enemy.Center, enemy.Level.ActiveHero.Center) == null)
                    {
                        enemy.WaitTime = 0;
                        esm.BehaviorState = new TrackState(esm);
                    }
                        
                }
            }
            
        }
    }
}
