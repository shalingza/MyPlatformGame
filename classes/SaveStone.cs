using Godot;
using System;
using System.Transactions;

public partial class SaveStone : Interactable 
{
	[Export] AnimationPlayer animationplayer;
    public override void Interact()
	{

        base.Interact();
		animationplayer.Play("activated");
        GetNode<SoundManager>("/root/SoundManager").PlaySFX("Save");
        var game = GetNode<Game>("/root/Game");
        game.SaveGame();
    }








	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
