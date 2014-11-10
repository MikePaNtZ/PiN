using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroCelebrateState : HeroMainState
    {
        public HeroCelebrateState(HeroStateMachine HSM) : base(HSM)
        {
            hero.sprite.LoadAnimation(hero.CelebrateAnimation);
            hero.InvincibilityFrames = 0;
            System.Diagnostics.Debug.WriteLine("CelebrateState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            //don't update at exit
        }
        public override void OnHit(GameObject hitBy)
        {
            //don't get hit
        }
    }
}
