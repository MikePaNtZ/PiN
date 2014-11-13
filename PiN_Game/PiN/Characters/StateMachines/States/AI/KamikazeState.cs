using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class KamikazeState : BehaviorState
    {
        public KamikazeState(EnemyStateMachine ESM)
            : base(ESM)
        {
            esm.ShooterState = new EnemyFiringState(esm);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            enemy.Target = enemy.Level.ActiveHero.Center;

            if (enemy.LineOfSightToHero.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float posX = enemy.Position.X + enemy.BoundingRectangle.Width / 2 * (int)enemy.FaceDirection;
            int tileX = (int)Math.Floor(posX / enemy.Level.TileWidth) - (int)enemy.FaceDirection;
            int tileY = (int)Math.Floor(enemy.Position.Y / enemy.Level.TileHeight);
            // move in the current direction.
            enemy.Move(enemy.FaceDirection);

            if (enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY - 1) == TileCollision.Impassable ||
                    enemy.Level.GetCollision(tileX + (int)enemy.FaceDirection, tileY) == TileCollision.Passable)
            {
                esm.MainState = new EnemyJumpingState(esm);
            }
        }
    }
}
