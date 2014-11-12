using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroShooterState : ShooterState
    {
        protected HeroStateMachine hsm;
        protected Hero hero;

        public HeroShooterState(HeroStateMachine HSM)
            : base(HSM)
        {
            this.hsm = (HeroStateMachine)sm;
            hero = (Hero)character;
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            hero.Weapon.UpdateWeaponState(gameInputs.MouseInput.Position);

            
        }
    }
}
