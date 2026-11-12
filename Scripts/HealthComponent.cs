using Godot;

/// <summary>Хранит здоровье и сообщает о его изменении и единственном событии смерти.</summary>
[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(float current, int maximum);
    [Signal] public delegate void DiedEventHandler();
    [Export] public int MaxHealth { get; set; } = 10;
    [Export] public int MetalReward { get; set; } = 15;
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public override void _Ready()
    {
        CurrentHealth = Mathf.Max(1, MaxHealth);
        GameManager.Instance?.Register(this);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }

    /// <summary>Повторные попадания в уже погибший объект не создают наград.</summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0 || !IsAlive) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        if (!IsAlive) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0 || !IsAlive) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }

    private void Die()
    {
        EmitSignal(SignalName.Died);
        GetParent().QueueFree();
    }
}
