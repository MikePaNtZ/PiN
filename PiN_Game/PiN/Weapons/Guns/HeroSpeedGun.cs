using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroSpeedGun : Gun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroSpeedGun(Texture2D loadedTexture, GameCharacter theShooter)
            : base(loadedTexture, theShooter)
        {
        }

        protected override void LoadContent()
        {
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");//placeholder
            // set the default weapon to a gun.
            theWeapon = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));
            crosshair = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Crosshair"));
            // load all bullets
            bullets = new GameObject[MAX_BULLETS];
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/HeroSpeed/Bullet"));
            }
        }

        protected override void checkBulletCollision(GameObject bullet, Rectangle bulletRect)
        {
            //Check for collisions with the enemies
            foreach (Enemy enemy in weaponWielder.Level.enemies)
            {
                if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                {
                    //We're going to want to put some enemy health reduction code here
                    //Enemy class needs a health member variable too
                    enemy.OnHit(weaponWielder);
                    bullet.IsAlive = false;
                }
            }
        }



    }
}

