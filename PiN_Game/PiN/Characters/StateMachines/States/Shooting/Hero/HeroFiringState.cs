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
                hero.Weapon.PerformNormalAttack();
            else if (gameInputs.MouseState.LeftButton != ButtonState.Pressed && gameInputs.PreviousMouseState.LeftButton != ButtonState.Pressed)
                hsm.ShooterState = new HeroAimingState(hsm);

        }
    }
}
