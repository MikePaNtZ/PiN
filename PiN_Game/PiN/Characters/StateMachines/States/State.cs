using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class State
    {
        protected CharacterStateMachine sm;
        protected GameCharacter character;

        public State(CharacterStateMachine sm)
        {
            this.sm = sm;
            character = sm.Character;
        }

        public virtual void Reset()
        {
            //implement in derived class
        }

        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
            //implement in derived class
        }
        public virtual void OnHit(GameObject hitBy)
        {
            //implement in derived class
        }

        public virtual void OnKilled(GameObject killedBy)
        {
            //implement in derived class
        }

        public virtual void OnReachedExit()
        {
            //implement in derived class
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //implement in derived class
        }
    }
}
