
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    class Bullet : GameObject
    {
        protected Gun gun;
        protected float bulletSpeed;

        /// <summary>
        /// Bullet constructor
        /// </summary>
        public Bullet(Gun theGun)
            : base()
        {
            gun = theGun;
        }
        public virtual float BulletSpeed 
        {
            get
            {
                return bulletSpeed;
            }
            set
            {
                bulletSpeed = value;
            }
        }

        public void Update()
        {
            //Only update them if they're alive
            if (IsAlive)
            {
                //Move our bullet based on its velocity
                Position += Velocity;

                //Rectangle the size of the screen so bullets that
                //fly off screen are deleted.
                Rectangle screenRect = new Rectangle(0, 0, (int)gun.WeaponWielder.Level.Camera.Position.X + gun.WeaponWielder.Level.Camera.ViewPort.Width, (int)gun.WeaponWielder.Level.Camera.Position.Y + gun.WeaponWielder.Level.Camera.ViewPort.Height * 2);
                if (!screenRect.Contains(new Point(
                    (int)Position.X,
                    (int)Position.Y)))
                {
                    IsAlive = false;
                    return;
                }

                //Collision rectangle for each bullet -Will also be
                //used for collisions with enemies.
                Rectangle bulletRect = new Rectangle(
                    (int)Position.X - Texture.Width * 2,
                    (int)Position.Y - Texture.Height * 2,
                    Texture.Width * 4,
                    Texture.Height * 4);

                checkBulletCollision(bulletRect);

                //Everything below here can be deleted if you want
                //your bullets to shoot through all tiles.

                //Look for adjacent tiles to the bullet

                /*Removed bullet collisions with tiles*/
                /*No more heinous noises when the enemy is shooting adjacent to a tile*/

                //Rectangle bounds = new Rectangle(
                //    bulletRect.Center.X - 6,
                //    bulletRect.Center.Y - 6,
                //    bulletRect.Width / 4,
                //    bulletRect.Height / 4);
                //int leftTile = (int)Math.Floor((float)bounds.Left / gun.WeaponWielder.Level.TileWidth);
                //int rightTile = (int)Math.Ceiling(((float)bounds.Right / gun.WeaponWielder.Level.TileWidth)) - 1;
                //int topTile = (int)Math.Floor((float)bounds.Top / gun.WeaponWielder.Level.TileHeight);
                //int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / gun.WeaponWielder.Level.TileHeight)) - 1;

                //// For each potentially colliding tile
                //for (int y = topTile; y <= bottomTile; ++y)
                //{
                //    for (int x = leftTile; x <= rightTile; ++x)
                //    {
                //        TileCollision collision = gun.WeaponWielder.Level.GetCollision(x, y);

                //        //If we collide with an Impassable or Platform tile
                //        //then delete our bullet.
                //        if (collision == TileCollision.Impassable ||
                //            collision == TileCollision.Platform)
                //        {
                //            if (bulletRect.Intersects(bounds))
                //                IsAlive = false;
                //        }
                //    }
                //}
            }
        }


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsAlive)
            {
                spriteBatch.Draw(Texture, Position, Color.White);
            }
        }

        public virtual void FireBullet()
        {
            if (gun.GunShotSound != null)
                gun.GunShotSound.Play();

            IsAlive = true;
            float theWeaponCos = (float)Math.Cos(rotation);
            float theWeaponSin = (float)Math.Sin(rotation);

            Position = gun.Position;

            Velocity = new Vector2(
                (float)Math.Cos(gun.Rotation),
                -(float)Math.Sin(gun.Rotation)) * BulletSpeed;
        }

        protected virtual void checkBulletCollision(Rectangle bulletRect)
        {
            //Check for collisions with the enemies
            foreach (Enemy enemy in gun.WeaponWielder.Level.enemies)
            {
                if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                {
                    //We're going to want to put some enemy health reduction code here
                    //Enemy class needs a health member variable too
                    enemy.OnHit(gun.WeaponWielder);
                    IsAlive = false;
                }
            }
        }

    }
}