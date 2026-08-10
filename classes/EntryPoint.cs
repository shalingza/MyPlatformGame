using Godot;
using System;

public partial class EntryPoint : Marker2D
{
    [Export]public Player.Direction direction = Player.Direction.Right;
    public override void _Ready()
	{
		AddToGroup("entrypoints");
        GD.Print($"入口点 {Name} 已添加到分组");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
