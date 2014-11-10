using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyIdleState : EnemyMainState
    {
        public EnemyIdleState(EnemyStateMachine ESM)
            : base(ESM)
        {
            enemy.sprite.LoadAnimation(enemy.IdleAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            if (Math.Abs(enemy.Velocity.X) - 0.02f > 0)
            {
                esm.MainState = new EnemyRunningState(esm);
            }
        }
    }
}

