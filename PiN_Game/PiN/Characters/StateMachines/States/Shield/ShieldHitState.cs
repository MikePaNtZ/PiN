using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class ShieldHitState : ShieldState
    {
        public ShieldHitState(HeroStateMachine HSM)
            : base(HSM)
        {
            hero.ShieldHitTime = hero.MaxShieldHitTime;
            System.Diagnostics.Debug.WriteLine("ShieldHitState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            hero.ShieldHitTime -= gameTime.ElapsedGameTime.Milliseconds / 1000F;

            if ((gameInputs.MouseState.RightButton != ButtonState.Pressed && gameInputs.PreviousMouseState.RightButton != ButtonState.Pressed) ||
                hero.IsAttacking)
            {
                hsm.ShieldState = new NotBlockingState(hsm);
                hero.ShieldHitTime = 0;
            }
            else if (hero.ShieldHitTime <= 0)
                hsm.ShieldState = new BlockingState(hsm);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float s = (float)gameTime.TotalGameTime.TotalSeconds * 200.0f;
            int colorIdx = (int)s % hero.isHitColors.Length;
            spriteBatch.Draw(hero.ShieldSprite, hero.rectangle, hero.isHitColors[colorIdx]);
        }
    }
}
