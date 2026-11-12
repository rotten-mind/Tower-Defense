using Godot;

/// <summary>Строительство перед персонажем: выбор сцены, безопасный предпросмотр, проверка места и цены.</summary>
[GlobalClass]
public partial class BuildComponent : Node
{
    [Export] public Godot.Collections.Array<PackedScene> TowerScenes { get; set; } = new();
    [Export] public Godot.Collections.Array<string> TowerNames { get; set; } = new() { "Обычная", "Взрывная", "Замораживающая" };
    [Export] public float BuildDistance { get; set; } = 3;
    [Export] public float FootprintRadius { get; set; } = 1;
    [Export] public float ArenaHalfSize { get; set; } = 19;
    [Export] public Color ValidColor { get; set; } = new(0.2f, 1, 0.5f, 0.4f);
    [Export] public Color InvalidColor { get; set; } = new(1, 0.15f, 0.15f, 0.4f);
    public bool IsBuilding { get; private set; }
    public int SelectedIndex { get; private set; }
    public int SelectedCost { get; private set; }
    public string SelectedName => TowerNames[SelectedIndex];
    public bool IsPlacementValid { get; private set; }
    private Node3D _player;
    private Node3D _preview;
    private StandardMaterial3D _previewMaterial;
    private Vector3 _aim = Vector3.Forward;

    public override void _Ready() => _player = (Node3D)GetParent();

    public void SetAim(Vector3 direction)
    {
        if (direction.LengthSquared() > 0.001f) _aim = direction.Normalized();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance?.IsPlaying != true)
        {
            StopBuilding();
            return;
        }
        if (Input.IsActionJustPressed("cancel_build"))
        {
            StopBuilding();
            return;
        }
        bool buildPressed = Input.IsActionJustPressed("build");
        if (buildPressed && !IsBuilding)
        {
            IsBuilding = true;
            CreatePreview();
            buildPressed = false;
        }
        // Цифры меняют тип только после открытия режима клавишей E.
        if (!IsBuilding) return;
        for (int i = 0; i < TowerScenes.Count; i++)
        {
            if (!Input.IsActionJustPressed($"tower_{i + 1}")) continue;
            SelectedIndex = i;
            CreatePreview();
        }
        if (_preview == null) return;
        Vector3 position = _player.GlobalPosition + _aim * BuildDistance;
        position.Y = 0;
        _preview.GlobalPosition = position;
        IsPlacementValid = CheckPosition(position);
        _previewMaterial.AlbedoColor = IsPlacementValid && GameManager.Instance.Metal >= SelectedCost ? ValidColor : InvalidColor;
        if (!buildPressed) return;
        if (!IsPlacementValid)
        {
            GameManager.Instance.ShowNotice("Здесь нельзя строить: занято или за границей поля");
            return;
        }
        if (!GameManager.Instance.TrySpendMetal(SelectedCost)) return;
        var tower = TowerScenes[SelectedIndex].Instantiate<Node3D>();
        tower.Position = position;
        GetTree().CurrentScene.AddChild(tower);
        GameManager.Instance.ShowNotice($"Построена башня: {SelectedName}");
        StopBuilding();
    }

    private bool CheckPosition(Vector3 position)
    {
        if (Mathf.Abs(position.X) + FootprintRadius > ArenaHalfSize || Mathf.Abs(position.Z) + FootprintRadius > ArenaHalfSize) return false;
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = new CylinderShape3D { Radius = FootprintRadius, Height = 2 },
            Transform = new Transform3D(Basis.Identity, position + Vector3.Up),
            CollisionMask = 3,
            CollideWithAreas = false
        };
        return _player.GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
    }

    /// <summary>Копирует только меши: у призрака нет скриптов, зон, коллизий и событий смерти.</summary>
    private void CreatePreview()
    {
        _preview?.QueueFree();
        _preview = new Node3D { Name = "TowerPreview" };
        _previewMaterial = new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = ValidColor
        };
        var source = TowerScenes[SelectedIndex].Instantiate<Node3D>();
        SelectedCost = Mathf.Max(0, source.GetNode<BuildableComponent>("BuildableComponent").MetalCost);
        CopyMeshes(source, _preview, Transform3D.Identity);
        source.Free();
        GetTree().CurrentScene.AddChild(_preview);
    }

    private void CopyMeshes(Node source, Node3D destination, Transform3D parentTransform)
    {
        Transform3D transform = source is Node3D node ? parentTransform * node.Transform : parentTransform;
        if (source is MeshInstance3D mesh)
            destination.AddChild(new MeshInstance3D { Mesh = mesh.Mesh, Transform = transform, MaterialOverride = _previewMaterial });
        foreach (Node child in source.GetChildren()) CopyMeshes(child, destination, transform);
    }

    private void StopBuilding()
    {
        IsBuilding = false;
        _preview?.QueueFree();
        _preview = null;
    }

    public override void _ExitTree() => StopBuilding();
}
