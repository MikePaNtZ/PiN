using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyRunningState : EnemyMainState
    {
        public EnemyRunningState(EnemyStateMachine ESM)
            : base(ESM)
        {
            enemy.sprite.LoadAnimation(enemy.RunAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            if (Math.Abs(enemy.Velocity.X) - 0.02f <= 0)
            {
                esm.MainState = new EnemyIdleState(esm);
            }

            if (!enemy.Level.ActiveHero.IsAlive ||
                      enemy.Level.ReachedExit ||
                      enemy.Level.TimeRemaining == TimeSpan.Zero ||
                      enemy.WaitTime > 0)
            {
                esm.MainState = new EnemyIdleState(esm);
            }
        }
    }
}
