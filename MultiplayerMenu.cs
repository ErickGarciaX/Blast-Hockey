using Godot;
using System;

public partial class MultiplayerMenu : Control
{
    [Export] private string GameScenePath = "res://blast_hockey.tscn";

    private LineEdit _ipInput;

    public override void _Ready()
    {
        _ipInput = GetNode<LineEdit>("TextureRect/VBoxContainer/IpInput");

        GetNode<Button>("TextureRect/VBoxContainer/HostButton").Pressed += OnHostPressed;
        GetNode<Button>("TextureRect/VBoxContainer/JoinButton").Pressed += OnJoinPressed;
        GetNode<Button>("TextureRect/VBoxContainer/BackButton").Pressed += OnBackPressed;

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
    }

    private void OnHostPressed()
    { 
        NetworkManager.Instance.HostGame();
        GD.Print("Servidor esperando jugadores...");
    }

    private void OnJoinPressed()
    {
        string ip = _ipInput.Text;
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1"; // Default a localhost si está vacío
        NetworkManager.Instance.JoinGame(ip);
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Jugador conectado: {id}");

        // Solo el HOST decide cuándo iniciar
        if (Multiplayer.IsServer())
        {
            // Guardamos la ID real del cliente en el NetworkManager
            NetworkManager.Instance.Player2Id = id;

            GD.Print("Iniciando partida para todos...");
            // Llamamos a la RPC para que TODOS cambien de escena
            NetworkManager.Instance.Rpc(nameof(NetworkManager.LoadGameScene), GameScenePath);
        }
    }

    private void OnConnectedToServer()
    {
        GD.Print("Cliente conectado. Esperando al host para cambiar escena...");
    }


    private void OnBackPressed()
    {
        NetworkManager.Instance.ResetConnection();
        GetTree().ChangeSceneToFile("res://main_menu.tscn");
    }
}
