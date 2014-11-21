
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
 
namespace PiN
{

    class Weapon : GameObject
    {
        protected GameCharacter weaponWielder;

        public GameCharacter WeaponWielder
        {
            get
            {
                return weaponWielder;
            }
        }

        /// <summary>
        /// Weapon constructor
        /// </summary>
        public Weapon(GameCharacter weaponUser) : base()
        {
            weaponWielder = weaponUser;
            LoadContent();
        }

        /// <summary>
        /// Loads the weapon.
        /// </summary>
        protected virtual void LoadContent()
        {

        }

        public virtual void UpdateWeaponState(Vector2 attackTarget)
        {
            // base weapon class has no implementation for this. implement in a derived class
        }

        public virtual void PerformNormalAttack()
        {
            // base weapon class has no implementation for this. implement in a derived class
        }

        public virtual void PerformSpecialAttack()
        {
            // base weapon class has no implementation for this. implement in a derived class
        }
    }
}