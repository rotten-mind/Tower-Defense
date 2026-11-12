using Godot;
using System.Collections.Generic;

/// <summary>Единственный менеджер уровня: состояние матча, металл и учёт живых объектов.</summary>
public partial class GameManager : Node3D
{
    public enum MatchState { Playing, Victory, Defeat }
    public static GameManager Instance { get; private set; }
    [Export] public int StartingMetal { get; set; } = 40;
    [Export] public float NoticeDuration { get; set; } = 2.5f;
    public int Metal { get; private set; }
    public MatchState State { get; private set; } = MatchState.Playing;
    public bool IsPlaying => State == MatchState.Playing;
    public string ResultText { get; private set; } = "";
    public string Notice { get; private set; } = "";
    public HealthComponent PlayerHealth { get; private set; }
    public HealthComponent CoreHealth { get; private set; }
    private readonly HashSet<HealthComponent> _registered = new();
    private double _noticeTime;

    public int EnemyCount
    {
        get
        {
            int count = 0;
            foreach (HealthComponent health in _registered)
                if (GodotObject.IsInstanceValid(health) && health.IsAlive && health.GetParent().IsInGroup("Enemies")) count++;
            return count;
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
        Metal = Mathf.Max(0, StartingMetal);
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _Process(double delta)
    {
        _noticeTime -= delta;
        if (_noticeTime <= 0) Notice = "";
        if (!IsPlaying && Input.IsActionJustPressed("restart")) GetTree().ReloadCurrentScene();
    }

    /// <summary>Вызывается каждым HealthComponent; удаление без смерти награды не даёт.</summary>
    public void Register(HealthComponent health)
    {
        if (!_registered.Add(health)) return;
        Node actor = health.GetParent();
        if (actor.IsInGroup("PlayerCharacter")) PlayerHealth = health;
        if (actor.IsInGroup("Core")) CoreHealth = health;
        health.Died += () => OnActorDied(health, actor);
        health.TreeExiting += () => _registered.Remove(health);
    }

    private void OnActorDied(HealthComponent health, Node actor)
    {
        if (!IsPlaying || !_registered.Remove(health)) return;
        if (actor.IsInGroup("Core")) Finish(false, "Ядро уничтожено");
        else if (actor.IsInGroup("PlayerCharacter")) Finish(false, "Игрок погиб");
        else if (actor.IsInGroup("Enemies") || actor.IsInGroup("Towers"))
            Metal += Mathf.Max(0, health.MetalReward);
    }

    public bool TrySpendMetal(int amount)
    {
        if (!IsPlaying || amount < 0) return false;
        if (Metal < amount)
        {
            ShowNotice($"Недостаточно металла: нужно {amount}, есть {Metal}");
            return false;
        }
        Metal -= amount;
        return true;
    }

    public void ShowNotice(string message)
    {
        Notice = message;
        _noticeTime = NoticeDuration;
    }

    public void Finish(bool victory, string reason)
    {
        if (!IsPlaying) return;
        State = victory ? MatchState.Victory : MatchState.Defeat;
        ResultText = reason;
    }
}
