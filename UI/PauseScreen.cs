using Godot;
using System;

public partial class PauseScreen : Control
{
	[Export] Button resume;
    bool isPaused = false;

    public override void _Ready()
	{
		Hide();
        var sound = GetNode<SoundManager>("/root/SoundManager");
		sound.SetupUISound(this);
		VisibilityChanged += () => GetTree().Paused =Visible;//设置成根据此界面是否显示来控制游戏是否暂停
    }

	public void ShowPause()
	{
       
        Show();
		resume.GrabFocus();

	}
    public override void _Input(InputEvent @event)
	{

        if (@event is InputEventScreenTouch touch && touch.Pressed)
        {
            if (Input.IsActionJustPressed("pause"))
            {
                Visible = !Visible;
                GetWindow().SetInputAsHandled();
            }
        }
        else if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            Visible = !Visible;
            GetWindow().SetInputAsHandled();
        }
    }

    void OnResumePressed()
	{
		Hide();
	}
    void OnQuitPressed()
	{
        var game = GetNode<Game>("/root/Game");
		game.BackToTitle();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
