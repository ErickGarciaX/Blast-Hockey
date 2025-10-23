using Godot;
using System;

public partial class Player2 : RigidBody2D
{
    [Export] public float Speed = 600f;

    private Vector2 _startPosition;

    private bool _isFrozen = false; 
    private bool _isTeleporting = false; 
    public override void _Ready()
    {
        GravityScale = 0;

        _startPosition = GlobalPosition;
    }   


    public override void _PhysicsProcess(double delta)
    {

        if (_isFrozen)
        {
            LinearVelocity = Vector2.Zero;
            return;
        }

        Vector2 input = new(
            Input.GetActionStrength("p2_right") - Input.GetActionStrength("p2_left"),
            Input.GetActionStrength("p2_down") - Input.GetActionStrength("p2_up")
        );

        if (input.Length() > 0)
        {
            // Solo normaliza si hay input
            LinearVelocity = input.Normalized() * Speed;
        }
        else
        {
            // Si no hay input, frena el jugador
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
