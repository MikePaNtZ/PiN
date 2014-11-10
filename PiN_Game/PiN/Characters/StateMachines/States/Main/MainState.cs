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
            if (character.IsPoweredUp)
                character.PowerUpTime = Math.Max(0.0f, character.PowerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

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
            if (character.Weapon.Rotation < 0)
                character.FaceDirection = FaceDirection.Left;
            else if (character.Weapon.Rotation > 0)
                character.FaceDirection = FaceDirection.Right;

            character.determineColor(gameTime);
            character.sprite.Draw(gameTime, spriteBatch, character.Position, character.Flip, character.Color);
        }
    }
}
