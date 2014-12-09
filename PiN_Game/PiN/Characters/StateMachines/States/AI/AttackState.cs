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
            enemy.Target = enemy.Level.ActiveHero.Center;
            esm.ShooterState = new EnemyFiringState(esm);
            enemy.AttackTime = enemy.MaxAttackTime;
            enemy.AttackWaitTime = enemy.MaxAttackWaitTime;
            
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            enemy.Target = enemy.Level.ActiveHero.Center;
            enemy.lineIntersectDistance = Collision.RayCastCollidesWithLevel(enemy.Center, enemy.Target);

            if (enemy.Health <= enemy.MaxHealth * enemy.KamikazeThresholdPercent)
            {
                esm.BehaviorState = new KamikazeState(esm);
                return;
            }

            if (!enemy.IsOnGround)
                enemy.Move(enemy.FaceDirection);

            if (enemy.CanSeeTarget && Math.Abs(enemy.LineOfSightToTarget.X) <= enemy.MaxAttackDistance)
            {
                //if (enemy.LineOfSightToTarget.X * (int)enemy.FaceDirection < 0) //make sure enemy is facing the right direction
                    //enemy.FaceDirection = (FaceDirection)(-(int)enemy.FaceDirection); //if not turn around

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
            }
            else
                esm.BehaviorState = new TrackState(esm);
            
            //if (Math.Abs(enemy.LineOfSightToHero.X) > enemy.MaxAttackDistance || Collision.RayCastCollidesWithLevel(enemy.Center, enemy.Target) != null)// player moved outside of attacking range then track
                //esm.BehaviorState = new TrackState(esm);
        }
    }
}
