using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class AttackState : BehaviorState
    {
        public AttackState(EnemyStateMachine ESM)
            : base(ESM)
        {
            esm.ShooterState = new EnemyFiringState(esm);
            enemy.AttackTime = enemy.MaxAttackTime;
            enemy.AttackWaitTime = enemy.MaxAttackWaitTime;
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            if (enemy.LineOfSightToHero.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

            if (enemy.IsAttacking)
            {
                enemy.AttackTime = Math.Max(0.0f, enemy.AttackTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (enemy.AttackTime <= 0.0f)
                {
                    esm.ShooterState = new EnemyAimingState(esm);
                    enemy.AttackWaitTime = enemy.MaxAttackWaitTime;
                }
                
            }
            else
            {
                enemy.AttackWaitTime = Math.Max(0.0f, enemy.AttackWaitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (enemy.AttackWaitTime <= 0.0f)
                {
                    esm.ShooterState = new EnemyFiringState(esm);
                    enemy.AttackTime = enemy.MaxAttackTime;
                }
            }
            if (Math.Abs(enemy.LineOfSightToHero.X) > enemy.MaxAttackDistance)// player moved outside of attacking range then track
                esm.BehaviorState = new TrackState(esm);
        }
    }
}
