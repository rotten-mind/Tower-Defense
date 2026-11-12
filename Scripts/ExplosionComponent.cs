using Godot;
using System.Collections.Generic;

/// <summary>При попадании наносит урон всем подходящим телам в радиусе взрыва.</summary>
[GlobalClass]
public partial class ExplosionComponent : DamagingComponent
{
    [Export] public float BlastRadius { get; set; } = 3.2f;
    [Export] public int MaximumTargets { get; set; } = 128;
    [Export] public float EffectDuration { get; set; } = 0.35f;
    [Export] public Color EffectColor { get; set; } = new(1, 0.65f, 0.12f, 0.35f);

    public override bool ApplyDamage(Node3D target)
    {
        if (!CanDamage(target)) return false;
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = BlastRadius },
            Transform = new Transform3D(Basis.Identity, GlobalPosition),
            CollisionMask = CollisionMask,
            CollideWithAreas = false
        };
        var hit = new HashSet<Node3D> { target };
        foreach (var result in GetWorld3D().DirectSpaceState.IntersectShape(query, MaximumTargets))
            if (result["collider"].AsGodotObject() is Node3D body) hit.Add(body);
        foreach (Node3D body in hit) base.ApplyDamage(body);

        var effect = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = BlastRadius, Height = BlastRadius * 2 },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = EffectColor,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            },
            Position = GlobalPosition,
            Scale = Vector3.One * 0.15f
        };
        GetTree().CurrentScene.AddChild(effect);
        var tween = effect.CreateTween();
        tween.TweenProperty(effect, "scale", Vector3.One, EffectDuration);
        tween.TweenCallback(Callable.From(effect.QueueFree));
        return true;
    }
}
