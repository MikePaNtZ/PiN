using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class BehaviorState : State
    {

        protected EnemyStateMachine esm;
        protected Enemy enemy;
        public BehaviorState(EnemyStateMachine esm)
            : base(esm)
        {
            this.esm = (EnemyStateMachine)sm;
            enemy = (Enemy)character;
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (enemy.Health <= enemy.MaxHealth * enemy.KamikazeThresholdPercent)
            {
                esm.BehaviorState = new KamikazeState(esm);
                return;
            }
                
        }
    }
}
