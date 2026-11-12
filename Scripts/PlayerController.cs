using Godot;

/// <summary>Переводит ввод в команды переиспользуемым компонентам движения и оружия.</summary>
[GlobalClass]
public partial class PlayerController : Node
{
    [Export] public float AimHeight { get; set; } = 0.9f;
    private Node3D _player;
    private MoveComponent _movement;
    private ShootComponent _weapon;
    private BuildComponent _builder;

    public override void _Ready()
    {
        _player = (Node3D)GetParent();
        _movement = _player.GetNode<MoveComponent>("MoveComponent");
        _weapon = _player.GetNode<ShootComponent>("ShootComponent");
        _builder = _player.GetNode<BuildComponent>("BuildComponent");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsPlaying != true) return;
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        _movement.SetInput(new Vector3(input.X, 0, input.Y));
        Camera3D camera = _player.GetViewport().GetCamera3D();
        Vector2 mouse = _player.GetViewport().GetMousePosition();
        var plane = new Plane(Vector3.Up, AimHeight);
        Vector3? point = plane.IntersectsRay(camera.ProjectRayOrigin(mouse), camera.ProjectRayNormal(mouse));
        if (point.HasValue)
        {
            Vector3 direction = point.Value - _player.GlobalPosition;
            direction.Y = 0;
            _weapon.SetManualAim(direction, !_builder.IsBuilding && Input.IsActionPressed("shoot"));
            _builder.SetAim(direction);
        }
    }
}
