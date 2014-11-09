using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class CharacterStateMachine
    {
        public GameCharacter Character;
        public MainState MainState;
        //public ShooterState ShootState;
        public CharacterStateMachine(GameCharacter character)
        {
            Character = character;
            Reset();
        }

        public virtual void Reset()
        {
            //to be implemented in derived class
        }

        public virtual void Update(GameTime gameTime, InputHandler gameInputs)
        {
            MainState.Update(gameTime, gameInputs);
        }
        public virtual void OnHit(GameObject hitBy)
        {
            MainState.OnHit(hitBy);
        }
        public virtual void OnKilled(GameObject killedBy)
        {
            MainState.OnKilled(killedBy);
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            MainState.Draw(gameTime, spriteBatch);
        }
    }
}
