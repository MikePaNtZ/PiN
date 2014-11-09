using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class MainState
    {
        protected CharacterStateMachine sm;
        protected GameCharacter character;

        public MainState(CharacterStateMachine sm)
        {
            this.sm = sm;
            character = sm.Character;
        }

        public virtual void Reset()
        {

        }

        public virtual void Update(GameTime gameTime, InputHandler input)
        {
            character.Movement = 0;
        }
        public virtual void OnHit(GameObject hitBy)
        {
            //implement in derived class
        }

        public virtual void OnKilled(GameObject killedBy)
        {
            System.Diagnostics.Debug.WriteLine("KilledBy");
            sm.MainState = new DeadState(sm, killedBy);
        }

        public virtual void OnReachedExit()
        {
            //implement in derived class
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            character.determineColor(gameTime);
            character.Sprite.Draw(gameTime, spriteBatch, character.Position, character.Flip, character.Color);
        }
    }
}
