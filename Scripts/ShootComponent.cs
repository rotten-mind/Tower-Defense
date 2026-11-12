using Godot;

/// <summary>Общая стрельба для автоматических башен, врагов и ручного оружия игрока.</summary>
[GlobalClass]
public partial class ShootComponent : Node
{
    [Export] public PackedScene BulletScene { get; set; }
    [Export] public float ShotInterval { get; set; } = 0.6f;
    [Export] public Marker3D MuzzleMarker { get; set; }
    [Export] public VisibilityComponent Visibility { get; set; }
    [Export] public float SpawnOffset { get; set; } = 0.25f;
    [Export] public bool ManualControl { get; set; }
    [Export] public string TargetGroup { get; set; } = "Enemies";
    private Node3D _rotatingVisual;
    private Vector3 _aimDirection = Vector3.Forward;
    private bool _trigger;
    private double _cooldown;

    public override void _Ready() => _rotatingVisual = GetParent().GetNode<Node3D>("MuzzleVisual");

    public void SetManualAim(Vector3 direction, bool trigger)
    {
        direction.Y = 0;
        if (direction.LengthSquared() > 0.001f) _aimDirection = direction.Normalized();
        _trigger = trigger;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsPlaying == false) return;
        _cooldown = System.Math.Max(_cooldown - delta, -delta);
        if (!ManualControl)
        {
            Node3D target = Visibility?.GetClosestTarget();
            if (target == null) return;
            Vector3 direction = target.GlobalPosition - _rotatingVisual.GlobalPosition;
            direction.Y = 0;
            if (direction.LengthSquared() < 0.001f) return;
            _aimDirection = direction.Normalized();
        }
        _rotatingVisual.LookAt(_rotatingVisual.GlobalPosition + _aimDirection, Vector3.Up);
        if (ManualControl && !_trigger || _cooldown > 0 || BulletScene == null) return;
        var bullet = BulletScene.Instantiate<Bullet>();
        bullet.Initialize(_aimDirection, ManualControl ? TargetGroup : Visibility.TargetGroup);
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = MuzzleMarker.GlobalPosition + _aimDirection * SpawnOffset;
        _cooldown += Mathf.Max(0.05f, ShotInterval);
    }
}
