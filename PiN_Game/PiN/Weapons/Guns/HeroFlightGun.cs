using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroFlightGun : HeroGun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroFlightGun(GameCharacter theShooter)
            : base(theShooter)
        {
        }

        protected override void LoadContent()
        {
            
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroFlight/Bullet");
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");
            base.LoadContent();
        }



    }
}

