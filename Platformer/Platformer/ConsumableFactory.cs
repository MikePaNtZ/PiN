using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Platformer
{
    static class ConsumableFactory
    {
        public static Consumable NewConsumable(Level level, Vector2 position, string consumableType)
        {
            switch (consumableType)
            {
                case "HealthConsumable":
                    return new HealthConsumable(level, position);
                case "PowerUp":
                    return new PowerUp(level, position);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
