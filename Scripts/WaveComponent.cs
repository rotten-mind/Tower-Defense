using Godot;

/// <summary>Создаёт конечные волны. Победа наступает после смерти всех врагов последней волны.</summary>
[GlobalClass]
public partial class WaveComponent : Node
{
    [Export] public PackedScene RangedEnemyScene { get; set; }
    [Export] public PackedScene CrawlerScene { get; set; }
    [Export] public int WaveCount { get; set; } = 5;
    [Export] public int InitialEnemyCount { get; set; } = 3;
    [Export] public int ExtraEnemiesPerWave { get; set; } = 2;
    [Export] public float PreparationTime { get; set; } = 12;
    [Export] public float BetweenWavesTime { get; set; } = 8;
    [Export] public float SpawnInterval { get; set; } = 1.4f;
    [Export] public float SpawnHalfSize { get; set; } = 18;
    [Export] public float SpawnSpread { get; set; } = 14;
    public int CurrentWave { get; private set; }
    public double Countdown { get; private set; }
    public bool IsWaiting => !_waveActive;
    private bool _waveActive;
    private int _remaining;
    private int _spawned;
    private double _spawnCooldown;
    private readonly RandomNumberGenerator _random = new();

    public override void _Ready()
    {
        _random.Randomize();
        Countdown = PreparationTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        GameManager game = GameManager.Instance;
        if (game?.IsPlaying != true) return;
        if (!_waveActive)
        {
            Countdown -= delta;
            if (Countdown > 0) return;
            CurrentWave++;
            _remaining = InitialEnemyCount + (CurrentWave - 1) * ExtraEnemiesPerWave;
            _spawned = 0;
            _spawnCooldown = 0;
            _waveActive = true;
        }
        if (_remaining > 0)
        {
            _spawnCooldown -= delta;
            if (_spawnCooldown > 0) return;
            SpawnEnemy();
            _remaining--;
            _spawned++;
            _spawnCooldown = SpawnInterval;
        }
        else if (game.EnemyCount == 0)
        {
            if (CurrentWave >= WaveCount) game.Finish(true, "Все волны отражены. Ядро спасено!");
            else
            {
                _waveActive = false;
                Countdown = BetweenWavesTime;
                game.ShowNotice("Волна отражена — подготовь оборону");
            }
        }
    }

    private void SpawnEnemy()
    {
        // Поочерёдные края исключают появление нескольких врагов в одной точке за один кадр.
        int edge = (_spawned + CurrentWave) % 4;
        float spread = _random.RandfRange(-SpawnSpread, SpawnSpread);
        Vector3 position = edge switch
        {
            0 => new Vector3(-SpawnHalfSize, 0, spread),
            1 => new Vector3(SpawnHalfSize, 0, spread),
            2 => new Vector3(spread, 0, -SpawnHalfSize),
            _ => new Vector3(spread, 0, SpawnHalfSize)
        };
        PackedScene scene = _spawned % 3 == 2 ? RangedEnemyScene : CrawlerScene;
        var enemy = scene.Instantiate<Node3D>();
        enemy.Position = position;
        GetTree().CurrentScene.AddChild(enemy);
    }
}
