using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroAimingState : HeroShooterState
    {
        public HeroAimingState(HeroStateMachine HSM) : base(HSM)
        {
            System.Diagnostics.Debug.WriteLine("AimingState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            if (gameInputs.MouseState.LeftButton == ButtonState.Pressed && hero.IsBlocking == false)
                hsm.ShooterState = new HeroFiringState(hsm);

            //TODO reloading

            //TODO switch gun
                
        }
    }
}
