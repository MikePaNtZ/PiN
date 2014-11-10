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
            enemy.Color = Color.Yellow;
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {

            if (enemy.LineOfSight.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Calculate tile position based on the side we are walking towards.
            float posX = enemy.Position.X + enemy.BoundingRectangle.Width / 2 * (int)enemy.FaceDirection;
            int tileX = (int)Math.Floor(posX / enemy.Level.TileWidth) - (int)enemy.FaceDirection;
            int tileY = (int)Math.Floor(enemy.Position.Y / enemy.Level.TileHeight);

            // If we are about to run into a wall then stop.
            if (enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY - 1) != TileCollision.Impassable)
            {
                //else move in the current direction
                enemy.Move(enemy.FaceDirection);
            }

            base.Update(gameTime, gameInputs);

            if (Math.Abs(enemy.LineOfSight.X) <= enemy.MaxAttackDistance)// player is in attacking distance then attack
                esm.BehaviorState = new AttackState(esm);
        }
    }
}
