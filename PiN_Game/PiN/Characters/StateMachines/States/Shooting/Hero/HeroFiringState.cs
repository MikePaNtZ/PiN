using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroFiringState : HeroShooterState
    {
        public HeroFiringState(HeroStateMachine HSM)
            : base(HSM)
        {
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            if (gameInputs.MouseState.LeftButton == ButtonState.Pressed && hero.IsBlocking == false)
            {
                Fire(gameTime);
            }
            else if (gameInputs.MouseState.LeftButton != ButtonState.Pressed && gameInputs.PreviousMouseState.LeftButton != ButtonState.Pressed)
            {
                alreadyFiring = false;
                hsm.ShooterState = new HeroAimingState(hsm);
            }

        }

        protected void Fire(GameTime gameTime)
        {
            if (alreadyFiring)
            {
                float gameUpdateRateSecs = (1.0f / (1000.0f / (float)gameTime.ElapsedGameTime.Milliseconds));
                int numOfUpdatesToWait = (int)((float)(1.0f/hero.Weapon.AttackRate) / gameUpdateRateSecs);
                if (countToFire == numOfUpdatesToWait)
                {
                    hero.Weapon.PerformNormalAttack();
                    countToFire = 0;
                }else
                {
                    countToFire++;
                }
            }
            else
            {
                hero.Weapon.PerformNormalAttack();
                alreadyFiring = true;
            }

        }

        private bool alreadyFiring;
        private int countToFire = 0;
    }
}
