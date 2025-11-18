using Godot;

public partial class NetworkManager : Node
{
    public static NetworkManager Instance;

    private ENetMultiplayerPeer _peer;

    public long Player2Id = 0;

    public override void _Ready()
    {
        Instance = this;

    }

    public void ResetConnection()
    {
        if (_peer != null)
        {
            _peer.Close(); 
            _peer = null;
        }

        Multiplayer.MultiplayerPeer = null;
    }

    public void HostGame()
    {
        ResetConnection();

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateServer(9000, 2);
        if (error != Error.Ok)
        {
            GD.PrintErr("Error al crear servidor: " + error);
            return;
        }
        Multiplayer.MultiplayerPeer = _peer;
        GD.Print("Servidor iniciado. Esperando jugadores...");
    }

    public void JoinGame(string ip)
    {
        ResetConnection();

        _peer = new ENetMultiplayerPeer();
        var error = _peer.CreateClient(ip, 9000);
        if (error != Error.Ok)
        {
            GD.PrintErr("Error al crear cliente: " + error);
            return;
        }
        Multiplayer.MultiplayerPeer = _peer;
        GD.Print("Uniéndose a: " + ip);
    }

    // --- NUEVA FUNCIÓN RPC ---
    // CallLocal = true hace que se ejecute tanto en el Host como en el Cliente
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void LoadGameScene(string scenePath)
    {
        GD.Print("Cargando escena: " + scenePath);
        GetTree().ChangeSceneToFile(scenePath);
    }
}
