using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyAimingState : EnemyShooterState
    {
        public EnemyAimingState(EnemyStateMachine ESM)
            : base(ESM)
        {
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            //TODO reloading

            //TODO switch gun

        }
    }
}
