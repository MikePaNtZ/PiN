using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class ShieldState : State
    {
        protected HeroStateMachine hsm;
        protected Hero hero;

        public ShieldState(HeroStateMachine HSM) : base(HSM)
        {
            hsm = HSM;
            hero = (Hero)HSM.Character;
        }

        public override void OnKilled(GameObject killedBy)
        {
            hsm.ShieldState = new DepletedState(hsm);
        }

        public override void OnReachedExit()
        {
            hsm.ShieldState = new DepletedState(hsm);
        }
    }
}
