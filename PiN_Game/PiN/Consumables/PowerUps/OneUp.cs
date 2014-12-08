#region File Description
//-----------------------------------------------------------------------------
// HealthConsumable.cs
// Health items picked up by player
//
// Microsoft XNA Community Program Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{
    class OneUp : Consumable
    {
        public OneUp(Level level, Vector2 position)
            : base(level, position)
        {
            color = Color.Gold;
            LoadContent();
        }

        /// <summary>
        /// Called when this item has been collected by a hero and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The hero who collected this item. He increases his health by the set amount
        /// </param>
        override public void OnCollected(Hero collectedBy)
        {
            collectedSound.Play();
            Level.HealOneHero();
        }
    }
}

