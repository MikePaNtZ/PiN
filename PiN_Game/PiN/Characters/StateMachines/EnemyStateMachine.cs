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
            ShooterState = new EnemyAimingState(this);
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            
            base.Update(gameTime, gameInputs);
            BehaviorState.Update(gameTime, gameInputs);
            
        }

    }
}

