using Godot;
using System;

public partial class Player1 : RigidBody2D
{
    [Export] public float Speed = 600f;

    private Vector2 _startPosition;


    private bool _isFrozen = false; 
    private bool _isTeleporting = false;


    private bool IsLocalMode => Multiplayer.MultiplayerPeer == null;


    public override void _Ready()
    {
        GravityScale = 0;
        _startPosition = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {

        if (!IsLocalMode && !IsMultiplayerAuthority())
            return;

        if (_isFrozen)
        {
            LinearVelocity = Vector2.Zero;
            return;
        }

        Vector2 input = new(
            Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
            Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")
        );

        if (input.Length() > 0)
        {
            LinearVelocity = input.Normalized() * Speed;
        }
        else
        {
            LinearVelocity = Vector2.Zero;
        }
    }

    public void ResetPosition()
    {

        _isFrozen = true;  
        _isTeleporting = true; 

    }

    public void StartGame()
    {
        _isFrozen = false;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // Comprueba si necesitamos teletransportarnos
        if (_isTeleporting)
        {
            // Obtiene el estado actual
            var newTransform = state.Transform;

            // Modifica la posición DENTRO del estado
            newTransform.Origin = _startPosition;

            // Asigna el nuevo estado
            state.Transform = newTransform;

            // Resetea también las velocidades
            state.LinearVelocity = Vector2.Zero;
            state.AngularVelocity = 0;

            // Desactiva el flag para teletransportar solo una vez
            _isTeleporting = false;
        }
        if (_isFrozen)
        {
            state.LinearVelocity = Vector2.Zero;
            state.AngularVelocity = 0;
        }

    }
}