using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class BlockingState : ShieldState
    {
        public BlockingState(HeroStateMachine HSM)
            : base(HSM)
        {
            System.Diagnostics.Debug.WriteLine("BlockingState");
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if ((gameInputs.MouseState.RightButton != ButtonState.Pressed && gameInputs.PreviousMouseState.RightButton != ButtonState.Pressed) ||
                hero.IsAttacking)
            {
                hsm.ShieldState = new NotBlockingState(hsm);
            }
        }
        public override void OnHit(GameObject hitBy)
        {
            //this is where we could deflect bullets etc

            if (hero.IsHit == false && hero.IsPoweredUp == false)
            {
                //ideally we could check exactly what hit us, bullet, enemy, bomb w/e
                //and take different actions
                if (hitBy is Enemy)
                {
                    hero.ShieldCharge -= 20;
                    //shield getting hit sound effect
                }
                else if (hitBy is GameObject) //ideally hit by bullet, but really anything for now
                {
                    hero.ShieldCharge -= 20;
                }
                else if (hitBy == null) //by hazard
                {
                    hero.ShieldCharge -= 10;
                }

                if (hero.ShieldCharge <= 0.0F)
                    hsm.ShieldState = new DepletedState(hsm);
                else
                    hsm.ShieldState = new ShieldHitState(hsm);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(hero.ShieldSprite, hero.rectangle, Color.White);
        }
    }
}
