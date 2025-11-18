using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] private string _gameScenePath = "res://blast_hockey.tscn";

    private void _on_singleplayer_button_pressed()
    {
        Multiplayer.MultiplayerPeer = null;
        GetTree().ChangeSceneToFile(_gameScenePath);
    }

    private void _on_multiplayer_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://multiplayer_menu.tscn");
    }

    private void _on_exit_button_pressed()
    {
        // Cierra el juego
        GetTree().Quit();
    }
}
