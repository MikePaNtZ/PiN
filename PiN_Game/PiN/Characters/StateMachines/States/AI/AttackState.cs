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
            enemy.Color = Color.Orange;
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            if (enemy.LineOfSight.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

            enemy.Weapon.UpdateWeaponState(enemy.Level.ActiveHero.Center);
            if (enemy.IsAttacking && !enemy.alreadyAttacking)
            {
                enemy.Weapon.PerformNormalAttack();
                enemy.alreadyAttacking = true;
                enemy.AttackWaitTime = enemy.MaxAttackWaitTime;
            }
            else if (enemy.alreadyAttacking)
            {
                enemy.AttackWaitTime = Math.Max(0.0f, enemy.AttackWaitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (enemy.AttackWaitTime <= 0.0f)
                {
                    enemy.alreadyAttacking = false;
                }
            }

            base.Update(gameTime, gameInputs);

            if (Math.Abs(enemy.LineOfSight.X) > enemy.MaxAttackDistance)// player moved outside of attacking range then track
                esm.BehaviorState = new TrackState(esm);
        }
    }
}
