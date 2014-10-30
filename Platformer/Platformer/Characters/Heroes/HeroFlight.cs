using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer
{
    class HeroFlight: Hero
    {
              public HeroFlight(Level level, Vector2 initialPosition, Texture2D defaultTexture)
            : base(level, initialPosition, defaultTexture)
        {
            LoadContent();
            Reset(initialPosition);
        }

        protected override void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Idle"), 0.1f, true);
            /*shieldPart1Animation is an image of just the shield alone; I couldn't get the shield to be overlayed onto the resetAfterHit
              I may have to do this later for the overdrive meter of the activeHero*/
            shieldPart1Animation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/ShieldPart1"), 0.1f, true);
            shieldAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Shield"), 0.1f, true); //load image for the shield


            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Die"), 0.1f, false);
            flinchAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Celebrate"), 0.1f, false); //placeholder

            // Calculate bounds within texture size.            
            // TODO It needs to be more clear what this is doing, and why it is done here. It is for collision detection.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");//placeholder
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/Powerup");

            // Load character's default weapon
            weapon = new Gun(Level.Content.Load<Texture2D>("Sprites/HeroFlight/Arm_Gun"), this);

        }


        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            base.Update(gameTime, gameInputs);

        }//end Update method

        protected override void determineAnimation(GameTime gameTime)
        {
            base.determineAnimation(gameTime);
        }
    }
}
