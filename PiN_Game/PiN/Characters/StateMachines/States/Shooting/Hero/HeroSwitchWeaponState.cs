using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class HeroSwitchWeaponState : HeroShooterState
    {
        public HeroSwitchWeaponState(HeroStateMachine HSM)
            : base(HSM)
        {
            System.Diagnostics.Debug.WriteLine("SwitchWeaponState");
        }
    }
}
