using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroMainState : MainState
    {
        protected HeroStateMachine hsm;
        protected Hero hero;
        public HeroMainState(HeroStateMachine hsm) : base(hsm)
        {
            this.hsm = (HeroStateMachine)sm;
            hero = (Hero)character;
        }

        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);
            if (hero.InvincibilityFrames > 0)
                hero.InvincibilityFrames -= gameTime.ElapsedGameTime.Milliseconds / 1000.0F;
        }

        public override void OnHit(GameObject hitBy)
        {
            if (hero.IsHit == false && hero.IsPoweredUp == false && hero.IsBlocking == false && hero.IsAlive)
            {
                //ideally we could check exactly what hit us, bullet, enemy, bomb w/e
                //and take different actions, maybe this should be a state
                if (hitBy is Enemy)
                {
                    hero.Health -= 25;
                    hero.HurtSound.Play();
                    hero.InvincibilityFrames = 1.0F;
                }
                else if (hitBy is Bullet) //by bullet
                {
                    hitBy.IsAlive = false;
                    hero.Health -= 10;
                    hero.HurtSound.Play();
                    hero.InvincibilityFrames = 0.5F;
                }
                else if (hitBy == null) //by hazard
                {
                    hero.Health -= 5;
                    hero.HurtSound.Play();
                    hero.InvincibilityFrames = 0.5F;
                }
            }
            

            if (hero.Health <= 0)
            {
                OnKilled(hitBy);
                hero.InvincibilityFrames = 0;
            }
        }

        public override void OnReachedExit()
        {
            hsm.MainState = new HeroCelebrateState(hsm);
        }
    }
}
