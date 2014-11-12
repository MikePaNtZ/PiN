using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyFiringState : EnemyShooterState
    {
        public EnemyFiringState(EnemyStateMachine ESM)
            : base(ESM)
        {
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            enemy.Weapon.PerformNormalAttack();

            //TODO reloading

            //TODO switch gun

        }
    }
}
