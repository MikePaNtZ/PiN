using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyJumpingState : EnemyMainState
    {
        public EnemyJumpingState(EnemyStateMachine ESM)
            : base(ESM)
        {
            enemy.sprite.LoadAnimation(enemy.JumpAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            if (enemy.IsOnGround)
            {
                if (Math.Abs(enemy.Velocity.X) - 0.02f > 0)
                {
                    esm.MainState = new EnemyRunningState(esm);
                }
                else
                    esm.MainState = new EnemyIdleState(esm);
            }
        }
    }
}
