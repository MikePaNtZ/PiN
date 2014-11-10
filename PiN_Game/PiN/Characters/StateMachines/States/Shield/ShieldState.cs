using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class ShieldState
    {
        protected HeroStateMachine hsm;
        protected Hero hero;

        public ShieldState(HeroStateMachine HSM)
        {
            hsm = HSM;
            hero = (Hero)HSM.Character;
        }
        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
        }

        public virtual void OnHit(GameObject hitBy)
        {
        }

        public virtual void OnKilled(GameObject killedBy)
        {
            hsm.ShieldState = new DepletedState(hsm);
        }

        public virtual void OnReachedExit()
        {
            hsm.ShieldState = new DepletedState(hsm);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }
    }
}
