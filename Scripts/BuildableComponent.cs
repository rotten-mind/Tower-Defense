using Godot;

/// <summary>Настройки строительства конкретной башни, редактируемые в инспекторе её сцены.</summary>
[GlobalClass]
public partial class BuildableComponent : Node
{
    [Export] public int MetalCost { get; set; } = 40;
}
