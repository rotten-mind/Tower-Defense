using Godot;
using System.Collections.Generic;

/// <summary>Запоминает вошедшие в зону тела и выбирает ближайшую живую цель нужной группы.</summary>
[GlobalClass]
public partial class VisibilityComponent : Area3D
{
    [Export] public string TargetGroup { get; set; } = "Enemies";
    private readonly HashSet<Node3D> _visibleTargets = new();

    public override void _Ready()
    {
        BodyEntered += body => { if (body.IsInGroup(TargetGroup)) _visibleTargets.Add(body); };
        BodyExited += body => _visibleTargets.Remove(body);
    }

    public Node3D GetClosestTarget()
    {
        Node3D closest = null;
        float best = float.PositiveInfinity;
        foreach (Node3D target in _visibleTargets)
        {
            if (!GodotObject.IsInstanceValid(target) || target.IsQueuedForDeletion()) continue;
            if (target.GetNodeOrNull<HealthComponent>("HealthComponent")?.IsAlive != true) continue;
            float distance = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
            if (distance >= best) continue;
            best = distance;
            closest = target;
        }
        return closest;
    }
}
