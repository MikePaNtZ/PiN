﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroStateMachine : CharacterStateMachine
    {
        public ShieldState ShieldState;

        public HeroStateMachine(Hero hero) : base(hero)
        {
        }

        public override void Reset()
        {
            MainState = new HeroIdleState(this);
            ShooterState = new HeroAimingState(this);
            ShieldState = new NotBlockingState(this);
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            ShieldState.Update(gameTime, gameInputs);
        }

        public override void OnHit(GameObject hitBy)
        {
            base.OnHit(hitBy);
            ShieldState.OnHit(hitBy);
        }

        public override void OnKilled(GameObject killedBy)
        {
            base.OnKilled(killedBy);
            ShieldState.OnKilled(killedBy);
        }

        public override void OnReachedExit()
        {
            MainState.OnReachedExit();
            ShooterState.OnReachedExit();
            ShieldState.OnReachedExit();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            ShieldState.Draw(gameTime, spriteBatch);
        }
        
    }
}
