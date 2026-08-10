using Godot;
using System;

public partial class GameOverScreen : Control
{
	[Export] AnimationPlayer animationplayer;
    [Export] AudioStream bgm;
    public override void _Input(InputEvent @event)
    {
		GetWindow().SetInputAsHandled();
		if (animationplayer .IsPlaying())
		{
			return;
		}
		if (@event is InputEventKey key && key.Pressed && !key.Echo ||
        @event is InputEventMouseButton mouse && mouse.Pressed ||
        @event is InputEventJoypadButton joy && joy.Pressed)
		{
            var game = GetNode<Game>("/root/Game");



			if (game.HasSave())
			{
				game.LoadGame();
			}
			else
			{
				game.BackToTitle();
			}
		}
	}
	public void ShowGameOver()
	{
		Show();
		SetProcessInput(true);
		animationplayer.Play("enter");
	}





	public override void _Ready()
	{
		Hide();
		SetProcessInput(false);
        GetNode<SoundManager>("/root/SoundManager").PlayBgm(bgm);

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
