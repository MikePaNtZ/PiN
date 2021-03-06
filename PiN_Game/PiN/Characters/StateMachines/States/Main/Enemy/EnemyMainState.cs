﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyMainState : MainState
    {
        protected EnemyStateMachine esm;
        protected Enemy enemy;
        public EnemyMainState(EnemyStateMachine esm)
            : base(esm)
        {
            this.esm = (EnemyStateMachine)sm;
            enemy = (Enemy)character;
        }

        public override void OnHit(GameObject hitBy)
        {
            System.Diagnostics.Debug.WriteLine("EnemyOnHit");
            if (enemy.IsPoweredUp == false)
            {
                //ideally we could check exactly what hit us, bullet, enemy, bomb w/e
                //and take different actions, maybe this should be a state
                if (hitBy is HeroStrength)
                {
                    enemy.Health -= 10;
                    enemy.HurtSound.Play();
                }
                else if (hitBy is HeroSpeed) //ideally hit by bullet, but really anything for now
                {
                    enemy.Health -= 1;
                    enemy.HurtSound.Play();
                }
                else if (hitBy is HeroFlight) //ideally hit by bullet, but really anything for now
                {
                    enemy.Health -= 3;
                    enemy.HurtSound.Play();
                }
                else if (hitBy is GameObject) //ideally hit by bullet, but really anything for now
                {
                    enemy.Health -= 1;
                    enemy.HurtSound.Play();
                }
                else if (hitBy == null) //by hazard
                {
                    enemy.Health -= 2;
                    enemy.HurtSound.Play();
                }
            }


            if (enemy.Health <= 0)
            {
                OnKilled(hitBy);
            }
        }

        public override void OnKilled(GameObject killedBy)
        {
            base.OnKilled(killedBy);
            enemy.SpawnRandomConsumable();
        }
    }
}

