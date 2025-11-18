using Godot;
using System;

public partial class Puck : RigidBody2D
{

    [Signal] public delegate void GoalScoredP1EventHandler();
    [Signal] public delegate void GoalScoredP2EventHandler();

    private Vector2 _startPosition;

    private bool _isFrozen = true;
    private bool _isTeleporting = false;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        GravityScale = 0;


        ContactMonitor = true;
        MaxContactsReported = 8;
        BodyEntered += OnPuckBodyEntered;
    }

    private void OnPuckBodyEntered(Node body)
    {

        if (Multiplayer.MultiplayerPeer != null && !Multiplayer.IsServer())
        {
            return;
        }

        // Verificamos si está congelado
        if (_isFrozen)
        {
            return;
        }

        // Verificamos los grupos
        if (body.IsInGroup("goal_p2"))
        {
            GD.Print("¡GOL DETECTADO en portería P2!");
            EmitSignal(SignalName.GoalScoredP1);
        }
        else if (body.IsInGroup("goal_p1"))
        {
            GD.Print("¡GOL DETECTADO en portería P1!");
            EmitSignal(SignalName.GoalScoredP2);
        }

    }


    public void ResetPosition()
    {
        _isFrozen = true;   // Activa la pausa
        _isTeleporting = true; // Activa la teletransportación


    }

    public void StartGame()
    {
        _isFrozen = false;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_isTeleporting)
        {
            var newTransform = state.Transform;
            newTransform.Origin = _startPosition;
            state.Transform = newTransform;
            state.LinearVelocity = Vector2.Zero;
            state.AngularVelocity = 0;
            _isTeleporting = false;
        }

        if (_isFrozen)
        {
            state.LinearVelocity = Vector2.Zero;
            state.AngularVelocity = 0;
        }
    }
}
