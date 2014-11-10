using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class NotBlockingState : ShieldState
    {
        public NotBlockingState(HeroStateMachine HSM) : base(HSM)
        {
            System.Diagnostics.Debug.WriteLine("NotBlockingState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (gameInputs.MouseState.RightButton == ButtonState.Pressed && 
                gameInputs.PreviousMouseState.RightButton == ButtonState.Pressed &&
                hsm.ShooterState.GetType() != typeof(HeroFiringState))
            {
                hsm.ShieldState = new BlockingState(hsm);
            }

            if (hero.ShieldCharge < hero.MaxShieldCharge)
                hero.ShieldCharge += (gameTime.ElapsedGameTime.Milliseconds / 1000F) * hero.ShieldChargeRate;
        }
    }
}
