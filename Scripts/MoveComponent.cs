using Godot;

/// <summary>Двигает CharacterBody3D по вводу игрока либо к цели. Хранит временное замедление.</summary>
[GlobalClass]
public partial class MoveComponent : Node
{
    [Export] public float Speed { get; set; } = 3;
    [Export] public bool PlayerControlled { get; set; }
    [Export] public float StopDistance { get; set; } = 6;
    [Export] public float CenterStopDistance { get; set; } = 0.4f;
    [Export] public Vector3 CenterPosition { get; set; } = Vector3.Zero;
    [Export] public float ArenaHalfSize { get; set; } = 19;
    private CharacterBody3D _body;
    private VisibilityComponent _visibility;
    private Vector3 _input;
    private float _slowMultiplier = 1;
    private double _slowTime;
    private MeshInstance3D _slowVisual;

    public override void _Ready()
    {
        _body = (CharacterBody3D)GetParent();
        _visibility = _body.GetNodeOrNull<VisibilityComponent>("VisibilityComponent");
        _slowVisual = _body.GetNodeOrNull<MeshInstance3D>("SlowVisual");
    }

    public void SetInput(Vector3 direction) => _input = direction.LimitLength();

    /// <summary>Повторные замедления обновляют срок и сохраняют сильнейший текущий эффект.</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        _slowMultiplier = Mathf.Min(_slowMultiplier, Mathf.Clamp(multiplier, 0, 1));
        _slowTime = Mathf.Max((float)_slowTime, duration);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsPlaying == false) return;
        _slowTime -= delta;
        if (_slowTime <= 0) _slowMultiplier = 1;
        if (_slowVisual != null) _slowVisual.Visible = _slowTime > 0;
        Vector3 direction = _input;
        if (!PlayerControlled)
        {
            Node3D target = _visibility?.GetClosestTarget();
            Vector3 destination = target?.GlobalPosition ?? CenterPosition;
            Vector3 offset = destination - _body.GlobalPosition;
            offset.Y = 0;
            float distance = target != null ? StopDistance : CenterStopDistance;
            direction = offset.Length() > distance ? offset.Normalized() : Vector3.Zero;
            if (_body.GetNodeOrNull<ShootComponent>("ShootComponent") == null && direction.LengthSquared() > 0)
            {
                Node3D visual = _body.GetNodeOrNull<Node3D>("MuzzleVisual");
                visual?.LookAt(visual.GlobalPosition + direction, Vector3.Up);
            }
        }
        _body.Velocity = direction * Speed * _slowMultiplier;
        _body.MoveAndSlide();
        if (PlayerControlled)
        {
            Vector3 position = _body.Position;
            position.X = Mathf.Clamp(position.X, -ArenaHalfSize, ArenaHalfSize);
            position.Z = Mathf.Clamp(position.Z, -ArenaHalfSize, ArenaHalfSize);
            _body.Position = position;
        }
    }
}
