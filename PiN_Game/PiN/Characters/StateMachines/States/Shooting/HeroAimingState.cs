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
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            System.Diagnostics.Debug.WriteLine("AimingState");

            if (gameInputs.MouseState.LeftButton == ButtonState.Pressed)
                hsm.ShooterState = new HeroFiringState(hsm);

            //TODO reloading

            //TODO switch gun
                
        }
    }
}
