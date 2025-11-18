using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class GameManager : Node2D
{
    [Export] private Label _gameOverLabel;
    [Export] private Button _menuButton;
    [Export] private string _mainMenuScenePath = "res://main_menu.tscn"; 

    private bool _isGameOver = false;
    private const int _maxScore = 7;

    private int _scorePlayer1 = 0;
    private int _scorePlayer2 = 0;

    [Export] private Puck _puck;

    [Export] private Player1 _player1;
    [Export] private Player2 _player2;


    [Export] private Label _scoreLabelP1;
    [Export] private Label _scoreLabelP2;

    [Export] private Label _countdownLabel;


    public override void _Ready()
    {
        GetTree().Paused = true;

        if (Multiplayer.MultiplayerPeer == null)
        {
            GD.Print("Modo: LOCAL");
            SetupLocalMode();
            GetTree().Paused = false; // En local despausamos ya
            return;
        }

        Multiplayer.PeerConnected += OnPeerConnected;
        SetupOnlineMode();

    }

    private void SetupLocalMode()
    {

        _puck.GoalScoredP1 += OnGoalScoredP1;
        _puck.GoalScoredP2 += OnGoalScoredP2;

        _scoreLabelP1.Text = _scorePlayer1.ToString();
        _scoreLabelP2.Text = _scorePlayer2.ToString();

        _menuButton.Pressed += OnMenuButtonPressed;

        ResetAllPositions();
    }

    private void SetupOnlineMode()
    {
        GD.Print("=== ONLINE MODE ===");

        long p1Id = 1; // El Host siempre es 1
        long p2Id = 0;

        if (Multiplayer.IsServer())
        {
            // Si soy Host, recupero la ID del cliente que guardé en el menú
            p2Id = NetworkManager.Instance.Player2Id;
        }
        else
        {
            // Si soy Cliente, yo soy el Player 2, así que uso mi propia ID
            p2Id = Multiplayer.GetUniqueId();
        }

        GD.Print($"Configurando Autoridades -> P1: {p1Id}, P2: {p2Id}");

        // Asignar autoridades
        _player1.SetMultiplayerAuthority((int)p1Id);
        _player2.SetMultiplayerAuthority((int)p2Id);
        _puck.SetMultiplayerAuthority(1); // El Host controla el puck

        // Eliminar objetos locales
        foreach (var n in GetTree().GetNodesInGroup("LocalOnly"))
            n.QueueFree();

        _menuButton.Pressed += OnMenuButtonPressed;

        // Ya estamos listos, iniciar cuenta atrás
        if (Multiplayer.IsServer())
        {
            _puck.GoalScoredP1 += OnGoalScoredP1;
            _puck.GoalScoredP2 += OnGoalScoredP2;
            // Esperar un frame o usar CallDeferred para asegurar que el cliente cargó
            Rpc(nameof(StartOnlineGame));
        }
    }


    private void OnPeerConnected(long id)
    {
        GD.Print("Jugador conectado: ", id);

        if (!Multiplayer.IsServer())
            return; // Solo el host controla el inicio

        // Host verifica cuando ya están los 2 jugadores
        if (Multiplayer.GetPeers().Count() >= 1)
        {
            GD.Print("Ambos jugadores listos → comenzando partida...");
            Rpc(nameof(StartOnlineGame));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void StartOnlineGame()
    {
        GD.Print("=== INICIANDO PARTIDA ONLINE ===");

        // Resetear posiciones y comenzar countdown
        GetTree().Paused = false;
        ResetAllPositions();

    }

    private void SpawnPlayer(long id)
    {
        Node2D playerToAssign;

        // Si es el primer jugador que se conecta →
        if (_player1.GetMultiplayerAuthority() == 0)
        {
            playerToAssign = _player1;
        }
        else
        {
            // El segundo jugador va a Player2
            playerToAssign = _player2;
        }

        // Asignar autoridad al jugador correcto:
        playerToAssign.SetMultiplayerAuthority((int)id);

        GD.Print($"Asignando autoridad del jugador {id} al nodo {playerToAssign.Name}");
    }

    private void SpawnPuck()
    {
        // El host siempre controla el puck
        _puck.SetMultiplayerAuthority(1);

        GD.Print("Autoridad del puck asignada al host.");
    }




    private void OnGoalScoredP1()
    {
        // Si estamos ONLINE (hay un peer conectado)
        if (Multiplayer.MultiplayerPeer != null)
        {
            Rpc(nameof(SyncScore), 1);
        }
        else
        {
            // Si estamos en LOCAL, llamamos a la función directo
            SyncScore(1);
        }
    }

    private void OnGoalScoredP2()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Rpc(nameof(SyncScore), 2);
        }
        else
        {
            SyncScore(2);
        }
    }

    // Esta función hace el trabajo sucio (sumar y resetear)
    // El atributo [Rpc] permite que sea llamada por red, pero NO impide usarla en local.
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void SyncScore(int playerWhoScored)
    {
        if (playerWhoScored == 1)
        {
            _scorePlayer1++;
            _scoreLabelP1.Text = _scorePlayer1.ToString();

            if (_scorePlayer1 >= _maxScore) EndGame("Player 1 Wins!");
            else ResetAllPositions();
        }
        else
        {
            _scorePlayer2++;
            _scoreLabelP2.Text = _scorePlayer2.ToString();

            if (_scorePlayer2 >= _maxScore) EndGame("Player 2 Wins!");
            else ResetAllPositions();
        }
    }

    private void ResetAllPositions()
    {
        _puck.ResetPosition();
        _player1.ResetPosition();
        _player2.ResetPosition();

        StartCountdown();
    }  

    private void EndGame(string winnerText)
    {
        _isGameOver = true;

        _countdownLabel.Hide();

        _gameOverLabel.Text = winnerText;
        _gameOverLabel.Show();
        _menuButton.Show();

        _puck.ResetPosition();
        _player1.ResetPosition();
        _player2.ResetPosition();
    }

    private void OnMenuButtonPressed()
    {
        // Vuelve al menú principal
        Multiplayer.MultiplayerPeer = null;
        GetTree().ChangeSceneToFile(_mainMenuScenePath);
    }

    private async void StartCountdown()
    {
        _puck.Hide();
        _countdownLabel.Show();

        _countdownLabel.Text = "3";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        _countdownLabel.Text = "2";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        _countdownLabel.Text = "1";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        _countdownLabel.Text = "¡GO!";
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        _player1.StartGame();
        _player2.StartGame();
        _puck.StartGame();


        _countdownLabel.Hide();
        _puck.Show();
    }
}