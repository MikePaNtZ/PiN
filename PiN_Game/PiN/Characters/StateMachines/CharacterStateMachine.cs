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
        public State MainState;
        public State ShooterState;

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
            ShooterState.Update(gameTime, gameInputs);
            MainState.Update(gameTime, gameInputs);
            
            
        }

        public virtual void OnHit(GameObject hitBy)
        {
            MainState.OnHit(hitBy);
        }

        public virtual void OnKilled(GameObject killedBy)
        {
            MainState.OnKilled(killedBy);
            ShooterState.OnKilled(killedBy);
        }

        public virtual void OnReachedExit() 
        {
            //to be implemented in derived class
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            ShooterState.Draw(gameTime, spriteBatch);
            MainState.Draw(gameTime, spriteBatch);   
        }
    }
}
