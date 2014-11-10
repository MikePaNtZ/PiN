using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyStateMachine : CharacterStateMachine
    {
        public State BehaviorState;

        public EnemyStateMachine(Enemy enemy)
            : base(enemy)
        {
        }

        public override void Reset()
        {
            MainState = new EnemyIdleState(this);
            BehaviorState = new SearchState(this);
            ShooterState = new State(this);
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            BehaviorState.Update(gameTime,gameInputs);
        }

        public override void OnHit(GameObject hitBy)
        {
            base.OnHit(hitBy);
        }

        public override void OnKilled(GameObject killedBy)
        {
            base.OnKilled(killedBy);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }

    }
}

