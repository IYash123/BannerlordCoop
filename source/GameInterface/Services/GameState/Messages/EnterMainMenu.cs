﻿using Common.Messaging;

namespace GameInterface.Services.GameState.Messages
{
    /// <summary>
    /// Goes to the main menu from any game state.
    /// </summary>
    public readonly struct EnterMainMenu : ICommand
    {
    }

    /// <summary>
    /// Reply to <seealso cref="EnterMainMenu"/>.
    /// </summary>
<<<<<<< HEAD
    public readonly struct MainMenuEntered : IEvent
=======
    public readonly struct MainMenuEntered : ICommand
>>>>>>> NetworkEvent-refactor
    {
    }
}
