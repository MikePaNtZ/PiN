using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroIdleState : HeroMainState
    {
        public HeroIdleState(HeroStateMachine HSM) : base(HSM)
        {
            hero.sprite.LoadAnimation(hero.IdleAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            if (gameInputs.KeyboardState.IsKeyDown(Keys.Left) || gameInputs.KeyboardState.IsKeyDown(Keys.A))
            {
                hero.Move(FaceDirection.Left);
                sm.MainState = new HeroRunningState(hsm);
            }
            else if (gameInputs.KeyboardState.IsKeyDown(Keys.Right) || gameInputs.KeyboardState.IsKeyDown(Keys.D))
            {
                hero.Move(FaceDirection.Right);
                sm.MainState = new HeroRunningState(hsm);
            }

            if (gameInputs.KeyboardState.IsKeyDown(Keys.Space) || gameInputs.KeyboardState.IsKeyDown(Keys.Up) || gameInputs.KeyboardState.IsKeyDown(Keys.W))
                sm.MainState = new HeroJumpingState(hsm);
        }
    }
}
