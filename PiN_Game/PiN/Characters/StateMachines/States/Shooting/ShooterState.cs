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

        public virtual void Update(GameTime gameTime, Vector2 target)
        {
            //implement in derived class
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
