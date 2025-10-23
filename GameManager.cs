using Godot;
using System;
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

        _puck.GoalScoredP1 += OnGoalScoredP1;
        _puck.GoalScoredP2 += OnGoalScoredP2;

        _scoreLabelP1.Text = _scorePlayer1.ToString();
        _scoreLabelP2.Text = _scorePlayer2.ToString();

        _menuButton.Pressed += OnMenuButtonPressed;

        ResetAllPositions();
    }


    private void OnGoalScoredP1()
    {
        _scorePlayer1++;

        _scoreLabelP1.Text = _scorePlayer1.ToString();

        if (_scorePlayer1 >= _maxScore)
        {
            EndGame("Player 1 Wins!");
        }
        else
        {
            ResetAllPositions(); 
        }
    }

    private void OnGoalScoredP2()
    {
        _scorePlayer2++;

        _scoreLabelP2.Text = _scorePlayer2.ToString();

        if (_scorePlayer2 >= _maxScore)
        {
            EndGame("Player 2 Wins!");
        }
        else
        {
            ResetAllPositions(); 
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