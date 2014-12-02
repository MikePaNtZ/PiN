using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroStrengthGun : HeroGun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroStrengthGun(GameCharacter theShooter)
            : base(theShooter)
        {
            bulletSpeed = 5.0f;
            attackRate = 2;
        }

        protected override void LoadContent()
        {
            
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroStrength/Bullet");
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");
            base.LoadContent();
        }



    }
}

