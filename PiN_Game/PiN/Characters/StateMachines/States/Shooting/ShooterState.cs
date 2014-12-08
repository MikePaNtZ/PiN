using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class ShooterState : State
    {
        public ShooterState(CharacterStateMachine sm) : base(sm)
        {
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            bool aimingRight = character.Weapon.Rotation <= Math.PI / 2.0f || character.Weapon.Rotation >= 3.0f * Math.PI / 2.0f;
            bool aimingLeft = character.Weapon.Rotation >= Math.PI / 2.0f || character.Weapon.Rotation <= 3.0f * Math.PI / 2.0f;

            if (aimingLeft)
                character.FaceDirection = FaceDirection.Left;
            if (aimingRight)
                character.FaceDirection = FaceDirection.Right;
        }

        public override void OnKilled(GameObject killedBy)
        {
            sm.ShooterState = new HolsterState(sm);
        }

        public override void OnReachedExit()
        {
            sm.ShooterState = new HolsterState(sm);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (character.IsAlive)
            {
                character.Weapon.Draw(gameTime, spriteBatch);
            }
        }
    }
}
