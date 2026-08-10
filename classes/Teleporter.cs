using Godot;
using System;

public partial class Teleporter : Interactable 
{
    [Export(PropertyHint.File, "*.tscn")]
    public string ScenePath { get; set; }
	[Export] string entrypoint; 

    public override async void Interact()
	{

		base.Interact();
        var game = GetNode<Game>("/root/Game");
		await game.ChangeScene(ScenePath, new Godot.Collections.Dictionary { 
			{ "entrypoint", entrypoint },
            { "duration", 1f }
          });
    }

    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
