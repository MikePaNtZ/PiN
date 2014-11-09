using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroRunningState : HeroMainState
    {
        public HeroRunningState(HeroStateMachine HSM) : base(HSM)
        {

            hero.sprite.LoadAnimation(hero.RunAnimation);
            System.Diagnostics.Debug.WriteLine("RunningState");

            //only do the run animation if velocity is above a threshold
            //to eliminate errors from sensitive keyboards or something
            //if (Math.Abs(hero.Velocity.X) - 0.02f > 0)
            //{
            //    hero.Sprite.LoadAnimation(hero.RunAnimation);
            //}
            //else
            //    hero.Sprite.LoadAnimation(hero.IdleAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

            bool isMoving = false;

            if (gameInputs.KeyboardState.IsKeyDown(Keys.Left) || gameInputs.KeyboardState.IsKeyDown(Keys.A))
            {
                hero.Move(FaceDirection.Left);
                isMoving = true;
            }
            else if (gameInputs.KeyboardState.IsKeyDown(Keys.Right) || gameInputs.KeyboardState.IsKeyDown(Keys.D))
            {
                hero.Move(FaceDirection.Right);
                isMoving = true;
            }
            if (gameInputs.KeyboardState.IsKeyDown(Keys.Space) || gameInputs.KeyboardState.IsKeyDown(Keys.Up) || gameInputs.KeyboardState.IsKeyDown(Keys.W))
                sm.MainState = new HeroJumpingState(hsm);
            else if (!isMoving)
                sm.MainState = new HeroIdleState(hsm);
        }
    }
}
