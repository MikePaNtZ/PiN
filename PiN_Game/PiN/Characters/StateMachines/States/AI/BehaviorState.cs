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
            //try
            //{
            //    if (enemy.CurrentPlatform != null && enemy.Level.ActiveHero.CurrentPlatform != null)
            //        enemy.Path = GlobalSolver.FindPath(enemy.CurrentPlatform, enemy.Level.ActiveHero.CurrentPlatform);
            //}
            //catch (ArgumentNullException e)
            //{
            //    System.Diagnostics.Debug.WriteLine(e);
            //}

            enemy.Target = enemy.Level.ActiveHero.Center;
            enemy.lineIntersectDistance = Collision.RayCastCollidesWithLevel(enemy.Center, enemy.Target);

        }
    }
}
