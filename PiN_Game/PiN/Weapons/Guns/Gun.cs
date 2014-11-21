
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    class Gun : Weapon
    {
        protected SoundEffect shootingSound;
        protected Texture2D bulletTexture;
        protected GameObject[] bullets;
        protected int MAX_BULLETS = 12;
        protected float bulletSpeed = 15.0f;
        protected Vector2 target;

        /// <summary>
        /// Gun constructor
        /// </summary>
        public Gun(GameCharacter theShooter)
            : base(theShooter)
        {
        }

        protected virtual float BulletSpeed {
            get {
                return bulletSpeed;
            }
        }

        /// <summary>
        /// Loads the weapon.
        /// </summary>
        protected override void LoadContent()
        {
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");
            texture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun");
            // load all bullets
            bullets = new GameObject[MAX_BULLETS];

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new GameObject();
                bullets[i].Texture = bulletTexture; 
            }
            
        }

        public override void UpdateWeaponState(Vector2 crosshairPosition)
        {
            // Shooting related updates
            target = crosshairPosition;
            position = weaponWielder.ArmPosition;

            // Updates the state of all bullets
            updateShooting();
            UpdateBullets();
            
        }

        public override void PerformNormalAttack()
        {
            FireBullet();
        }

        public override void PerformSpecialAttack()
        {
            // Haven't implemented a special gun attack yet....
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
//            spriteBatch.Draw(
//                texture,
//                position,
//                null,
//                Color.White,
//                rotation,
//                Center,
//                1.0f,
//                weaponWielder.Flip,
//                0);

            //Draw the bullets
            foreach (GameObject bullet in bullets)
            {
                if (bullet.IsAlive)
                {
                    spriteBatch.Draw(bullet.Texture,
                        bullet.Position, Color.White);
                }
            }

            XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, position, target, Color.Blue, 1);
            
        }


        private void updateShooting()
        {
            Vector2 aimDirection = position - target;
            aimDirection.Y *= -1.0f;
            aimDirection.Normalize();
            rotation = (float)Math.Atan2(aimDirection.Y, aimDirection.X) + (float)Math.PI;
        }

        private void FireBullet()
        {
            foreach (GameObject bullet in bullets)
            {
                if (!bullet.IsAlive)
                {

                    if (shootingSound != null)
                        shootingSound.Play();

                    bullet.IsAlive = true;
                    float theWeaponCos = (float)Math.Cos(rotation);
                    float theWeaponSin = (float)Math.Sin(rotation);

                    bullet.Position = new Vector2(
                        position.X,
                        position.Y);

                    bullet.Velocity = new Vector2(
                        (float)Math.Cos(rotation),
                        -(float)Math.Sin(rotation)) * BulletSpeed;
                }
            }
        }

        private void UpdateBullets()
        {
            //Check all of our bullets
            foreach (GameObject bullet in bullets)
            {
                //Only update them if they're alive
                if (bullet.IsAlive)
                {
                    //Move our bullet based on it's velocity
                    bullet.Position += bullet.Velocity;

                    //Rectangle the size of the screen so bullets that
                    //fly off screen are deleted.
                    Rectangle screenRect = new Rectangle(0, 0, (int)weaponWielder.Level.Camera.Position.X + weaponWielder.Level.Camera.ViewPort.Width, (int)weaponWielder.Level.Camera.Position.Y + weaponWielder.Level.Camera.ViewPort.Height * 2);
                    if (!screenRect.Contains(new Point(
                        (int)bullet.Position.X,
                        (int)bullet.Position.Y)))
                    {
                        bullet.IsAlive = false;
                        continue;
                    }

                    //Collision rectangle for each bullet -Will also be
                    //used for collisions with enemies.
                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.Position.X - bullet.Texture.Width * 2,
                        (int)bullet.Position.Y - bullet.Texture.Height * 2,
                        bullet.Texture.Width * 4,
                        bullet.Texture.Height * 4);

                    checkBulletCollision(bullet, bulletRect);

                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / weaponWielder.Level.TileWidth);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / weaponWielder.Level.TileWidth)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / weaponWielder.Level.TileHeight);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / weaponWielder.Level.TileHeight)) - 1;

                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = weaponWielder.Level.GetCollision(x, y);

                            //If we collide with an Impassable or Platform tile
                            //then delete our bullet.
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    bullet.IsAlive = false;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void checkBulletCollision(GameObject bullet, Rectangle bulletRect)
        {
            // No implementation in Gun Base class. It's up to derived gun classes 
            // to implement their own bullet collision.
        }


    }
}