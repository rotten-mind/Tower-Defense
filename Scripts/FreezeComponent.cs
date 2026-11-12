using Godot;

/// <summary>Наносит урон и временно замедляет цель через её компонент движения.</summary>
[GlobalClass]
public partial class FreezeComponent : DamagingComponent
{
    [Export(PropertyHint.Range, "0,1,0.05")] public float SpeedMultiplier { get; set; } = 0.35f;
    [Export] public float Duration { get; set; } = 2.5f;

    public override bool ApplyDamage(Node3D target)
    {
        if (!base.ApplyDamage(target)) return false;
        target.GetNodeOrNull<MoveComponent>("MoveComponent")?.ApplySlow(SpeedMultiplier, Duration);
        return true;
    }
}
