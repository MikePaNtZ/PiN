using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroReloadingState : HeroShooterState
    {
        public HeroReloadingState(HeroStateMachine HSM)
            : base(HSM)
        {
            System.Diagnostics.Debug.WriteLine("ReloadingState");
        }
    }
}
