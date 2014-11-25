using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class DepletedState : ShieldState
    {
        public DepletedState(HeroStateMachine HSM)
            : base(HSM)
        {
            hero.DepletedShieldTime = hero.MaxDepletedShieldTime;
            System.Diagnostics.Debug.WriteLine("DepletedState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (hero.IsAlive && hsm.MainState.GetType() != typeof(HeroCelebrateState))
            {
                hero.DepletedShieldTime -= gameTime.ElapsedGameTime.Milliseconds / 1000F;
                if (hero.DepletedShieldTime <= 0.0F)
                {
                    hsm.ShieldState = new NotBlockingState(hsm);
                }
            }
        }
    }
}
