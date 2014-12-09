using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class TrackState : BehaviorState
    {
        public TrackState(EnemyStateMachine ESM)
            : base(ESM)
        {
            esm.ShooterState = new EnemyAimingState(esm);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            if (enemy.Health <= enemy.MaxHealth * enemy.KamikazeThresholdPercent)
            {
                esm.BehaviorState = new KamikazeState(esm);
                return;
            }

            //if (enemy.LineOfSightToTarget.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                //enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Calculate tile position based on the side we are walking towards.
            float posX = enemy.Position.X;// +enemy.BoundingRectangle.Width / 2 * (int)enemy.FaceDirection;
            int tileX = (int)Math.Floor(posX / enemy.Level.TileWidth);// -(int)enemy.FaceDirection;
            int tileY = (int)Math.Floor(enemy.Position.Y / enemy.Level.TileHeight);
            // move in the current direction.
            enemy.Move(enemy.FaceDirection);

            if (enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY - 1) == TileCollision.Impassable ||
                    enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY) == TileCollision.Passable)
            {
                esm.MainState = new EnemyJumpingState(esm);
            }


            if (Math.Abs(enemy.LineOfSightToTarget.X) <= enemy.MaxAttackDistance && enemy.CanSeeTarget)// player is in attacking distance then attack
                esm.BehaviorState = new AttackState(esm);
            else if (Math.Abs(enemy.LineOfSightToTarget.X) > enemy.MinTrackDistance + 100)
                esm.BehaviorState = new SearchState(esm);
        }
    }
}
