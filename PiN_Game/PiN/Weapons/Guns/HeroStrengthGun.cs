using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroStrengthGun : Gun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroStrengthGun(GameCharacter theShooter)
            : base(theShooter)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroStrength/Bullet");
        }



    }
}

