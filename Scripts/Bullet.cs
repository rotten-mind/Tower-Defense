using Godot;

/// <summary>Только полёт и время жизни; столкновения и весь урон принадлежат дочерней зоне.</summary>
public partial class Bullet : Node3D
{
    [Export] public float Speed { get; set; } = 22;
    [Export] public float Lifetime { get; set; } = 3;
    private Vector3 _direction;
    private double _age;

    public void Initialize(Vector3 direction, string targetGroup)
    {
        _direction = direction.Normalized();
        GetNode<DamagingComponent>("DamagingComponent").SetTargetGroup(targetGroup);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsPlaying == false) return;
        Position += _direction * Speed * (float)delta;
        _age += delta;
        if (_age >= Lifetime) QueueFree();
    }
}
