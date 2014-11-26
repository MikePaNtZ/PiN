using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class EnemyShooterState : ShooterState
    {
        protected EnemyStateMachine esm;
        protected Enemy enemy;

        public EnemyShooterState(EnemyStateMachine HSM)
            : base(HSM)
        {
            esm = (EnemyStateMachine)sm;
            enemy = (Enemy)character;
        }

        //not sure if i need this
        public virtual void Update(GameTime gameTime, Vector2 target)
        {
            //implement in derived class
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            enemy.Weapon.UpdateWeaponState(enemy.Target, gameTime);
        }
    }
}