using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class MainState : State
    {

        public MainState(CharacterStateMachine sm) : base (sm)
        {
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (character.IsPoweredUp)
                character.PowerUpTime = Math.Max(0.0f, character.PowerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

            character.Movement = 0;
        }

        public override void OnKilled(GameObject killedBy)
        {
            System.Diagnostics.Debug.WriteLine("KilledBy");
            sm.MainState = new DeadState(sm, killedBy);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float deadBand = MathHelper.ToRadians(0.0f);
            if (character.Weapon.Rotation < deadBand)
                character.FaceDirection = FaceDirection.Left;
            if (character.Weapon.Rotation > -deadBand)
                character.FaceDirection = FaceDirection.Right;

            character.determineColor(gameTime);
            character.sprite.Draw(gameTime, spriteBatch, character.Position, character.Flip, character.Color);
        }
    }
}
