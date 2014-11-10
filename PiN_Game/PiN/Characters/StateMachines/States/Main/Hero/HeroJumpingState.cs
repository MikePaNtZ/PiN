using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroJumpingState : HeroMainState
    {
        public HeroJumpingState(HeroStateMachine HSM) : base(HSM)
        {
            hero.sprite.LoadAnimation(hero.JumpAnimation);
            character.JumpSound.Play();
            System.Diagnostics.Debug.WriteLine("JumpingState");
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

            if (isMoving && hero.IsOnGround)
                hsm.MainState = new HeroRunningState(hsm);
            else if (hero.IsOnGround)
                hsm.MainState = new HeroIdleState(hsm);
        }
    }
}
