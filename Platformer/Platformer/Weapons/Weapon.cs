
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
 
namespace Platformer
{

    class Weapon : GameObject
    {
        protected GameCharacter weaponWielder;
        protected GameObject theWeapon;

        /// <summary>
        /// Weapon constructor
        /// </summary>
        public Weapon(Texture2D loadedTexture, GameCharacter weaponUser) : base(loadedTexture)
        {
            weaponWielder = weaponUser;
            LoadContent();
        }

        /// <summary>
        /// Loads the weapon.
        /// </summary>
        protected virtual void LoadContent()
        {
            // this is essentially an abstract base class. set the weapon to null to force the user of
            // this class to properly derive from it.
            theWeapon = null;
        }

        public virtual void UpdateWeaponState(InputHandler gameInputs)
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
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            /// needs to be implemented by derived class.
        }

    }
}