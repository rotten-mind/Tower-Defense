using Godot;

/// <summary>Общая зона урона для снаряда и контактного врага; проверяет сторону цели.</summary>
[GlobalClass]
public partial class DamagingComponent : Area3D
{
    [Export] public float Damage { get; set; } = 2;
    [Export] public Godot.Collections.Array<string> TargetGroups { get; set; } = new() { "Enemies" };
    [Export] public bool OneShot { get; set; } = true;
    [Export] public float ContactInterval { get; set; } = 0.8f;
    private bool _consumed;
    private double _contactCooldown;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public void SetTargetGroup(string group)
    {
        TargetGroups = new() { group };
        CollisionMask = group == "Enemies" ? 2u : 1u;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (OneShot || GameManager.Instance?.IsPlaying == false) return;
        _contactCooldown -= delta;
        if (_contactCooldown > 0) return;
        _contactCooldown = Mathf.Max(0.05f, ContactInterval);
        foreach (Node3D body in GetOverlappingBodies()) ApplyDamage(body);
    }

    protected bool CanDamage(Node3D target)
    {
        if (!GodotObject.IsInstanceValid(target) || target.IsQueuedForDeletion()) return false;
        if (target.GetNodeOrNull<HealthComponent>("HealthComponent")?.IsAlive != true) return false;
        foreach (string group in TargetGroups)
            if (target.IsInGroup(group)) return true;
        return false;
    }

    /// <summary>Единая точка нанесения урона; наследники добавляют эффект к попаданию.</summary>
    public virtual bool ApplyDamage(Node3D target)
    {
        if (!CanDamage(target)) return false;
        target.GetNode<HealthComponent>("HealthComponent").TakeDamage(Damage);
        return true;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!OneShot || _consumed || GameManager.Instance?.IsPlaying == false) return;
        if (!ApplyDamage(body)) return;
        _consumed = true;
        DestroyAfterHit();
    }

    /// <summary>Удаляет одноразовый носитель целиком; Crawler продолжает существовать.</summary>
    protected virtual void DestroyAfterHit() => GetParent().QueueFree();
}
