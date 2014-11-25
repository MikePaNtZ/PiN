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
        Texture2D shootingTexture;
        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroFlightGun(GameCharacter theShooter)
            : base(theShooter)
        {
        }

        protected override void LoadContent()
        {
            shootingTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroFlight/Shoot");
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroFlight/Bullet");
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");
            base.LoadContent();
        }

        protected override void FireBullet()
        {
            foreach (Bullet bullet in bullets)
            {
                if (!bullet.IsAlive)
                {
                    bullet.FireBullet();
                    return;
                }

            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                crosshair.Texture,
                crosshair.Position,
                null,
                Color.White,
                crosshair.Rotation,
                crosshair.Center,
                1.0f,
                weaponWielder.Flip,
                0);

            base.Draw(gameTime, spriteBatch);
        }

    }
}

