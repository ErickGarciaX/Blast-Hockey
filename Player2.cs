using Godot;
using System;

public partial class Player2 : RigidBody2D
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
        // 1. Si estamos Online y NO soy el dueño de este muñeco, no hago nada (el Synchronizer lo mueve)
        if (!IsLocalMode && !IsMultiplayerAuthority())
            return;

        if (_isFrozen)
        {
            LinearVelocity = Vector2.Zero;
            return;
        }

        // --- LÓGICA DE INPUTS CAMBIADA ---
        Vector2 input = Vector2.Zero;

        if (IsLocalMode)
        {
            // MODO LOCAL: Usamos las teclas específicas del P2 (Flechas)
            input = new Vector2(
                Input.GetActionStrength("p2_right") - Input.GetActionStrength("p2_left"),
                Input.GetActionStrength("p2_down") - Input.GetActionStrength("p2_up")
            );
        }
        else
        {
            // MODO ONLINE: El cliente usa su teclado normal (WASD) para controlarse a sí mismo
            input = new Vector2(
                Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
                Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")
            );
        }
        // -------------------------------

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
