using Godot;

/// <summary>Показывает здоровье, металл, волны, строительство и итог матча.</summary>
public partial class GameHud : CanvasLayer
{
    private Label _stats;
    private Label _wave;
    private Label _build;
    private Label _notice;
    private Label _result;
    private PanelContainer _resultPanel;
    private WaveComponent _waves;
    private BuildComponent _builder;

    public override void _Ready()
    {
        _stats = GetNode<Label>("Margin/VBox/Stats");
        _wave = GetNode<Label>("Margin/VBox/Wave");
        _build = GetNode<Label>("Bottom/VBox/Build");
        _notice = GetNode<Label>("Margin/VBox/Notice");
        _resultPanel = GetNode<PanelContainer>("ResultCenter/Panel");
        _result = GetNode<Label>("ResultCenter/Panel/Result");
        _waves = GetParent().GetNode<WaveComponent>("WaveComponent");
        _builder = GetParent().GetNode<BuildComponent>("Player/BuildComponent");
    }

    public override void _Process(double delta)
    {
        GameManager game = GameManager.Instance;
        if (game == null) return;
        _stats.Text = $"МЕТАЛЛ: {game.Metal}     ИГРОК: {HealthText(game.PlayerHealth)}     ЯДРО: {HealthText(game.CoreHealth)}";
        _wave.Text = _waves.IsWaiting
            ? $"Волна {_waves.CurrentWave + 1}/{_waves.WaveCount} через {Mathf.CeilToInt((float)_waves.Countdown)} с"
            : $"ВОЛНА {_waves.CurrentWave}/{_waves.WaveCount}     Врагов на поле: {game.EnemyCount}";
        if (GodotObject.IsInstanceValid(_builder))
            _build.Text = _builder.IsBuilding
                ? $"{_builder.SelectedName} — {_builder.SelectedCost} металла\n1 — обычная | 2 — взрывная | 3 — замораживающая | E — поставить | Q — отмена"
                : "WASD — движение | Мышь — прицел | ЛКМ / Пробел — огонь\nE — строительство";
        _notice.Text = game.Notice;
        _notice.Visible = game.Notice.Length > 0;
        _build.Visible = game.IsPlaying;
        _resultPanel.Visible = !game.IsPlaying;
        if (!game.IsPlaying)
            _result.Text = $"{(game.State == GameManager.MatchState.Victory ? "ПОБЕДА" : "ПОРАЖЕНИЕ")}\n\n{game.ResultText}\n\nR — начать заново";
    }

    private static string HealthText(HealthComponent health) => GodotObject.IsInstanceValid(health) ? $"{health.CurrentHealth:0.##}/{health.MaxHealth}" : "0";
}
