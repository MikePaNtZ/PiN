using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HolsterState : ShooterState
    {
        public HolsterState(CharacterStateMachine SM)
            : base(SM)
        {
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            System.Diagnostics.Debug.WriteLine("HolsterState");
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //don't draw gun
        }
    }
}
