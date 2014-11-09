using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroStateMachine : CharacterStateMachine
    {
        public HeroStateMachine(Hero hero) : base(hero)
        {
        }

        public override void Reset()
        {
            MainState = new HeroIdleState(this);
        }

        public void OnReachedExit()
        {
            MainState.OnReachedExit();
        }
    }
}
