#region File Description
//-----------------------------------------------------------------------------
// EnemyState.cs
// Enumeration for the enemy finite state machine
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace Platformer
{
    /// <summary>
    /// Controls what state an enemy is in
    /// </summary>
    enum EnemyState
    {
        /// <summary>
        /// Enemy wanders around
        /// </summary>
        Search = 0,

        /// <summary>
        /// The enemy sees the player but is still too far away to attack
        /// </summary>
        Track = 1,

        /// <summary>
        /// Enemy attacks player
        /// </summary>
        Attack = 2,

        /// <summary>
        /// Enemy is low on health and charges kamikaze-style at player
        /// </summary>
        Kamikaze = 3,
    }
}