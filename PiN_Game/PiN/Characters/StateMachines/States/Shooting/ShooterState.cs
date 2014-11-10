using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class ShooterState
    {
        protected CharacterStateMachine sm;
        protected GameCharacter character;

        public ShooterState(CharacterStateMachine sm)
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

        }

        public virtual void Update(GameTime gameTime, Vector2 target)
        {

        }
        public virtual void OnHit(GameObject hitBy)
        {
            //implement in derived class
        }

        public virtual void OnKilled(GameObject killedBy)
        {
            sm.ShooterState = new HolsterState(sm);
        }

        public virtual void OnReachedExit()
        {
            sm.ShooterState = new HolsterState(sm);
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (character.IsAlive)
            {
                character.Weapon.Draw(gameTime, spriteBatch);
            }
        }
    }
}
