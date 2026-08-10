using Godot;
using System;

public partial class GameEndScreen : Control
{
    string[] Lines =
	{ 
		"恭喜打败boss",
		"感谢你的游玩",
		"欢迎再来一次"
	};
	int currentline = -1;
	Tween tween;
	[Export] Label label;
    [Export] AudioStream bgm;
    void ShowLine(int line)
	{
		currentline = line;
		tween = CreateTween();
        tween.SetEase(Tween.EaseType.InOut);//设置缓动类型
        tween.SetTrans(Tween.TransitionType.Sine);//设置过度曲线
		if (line >0)
		{
            tween.TweenProperty(label, "modulate", new Color(label.Modulate, 0), 1);
        }
		else
		{
            label.Modulate = new Color(label.Modulate, 0);
        }
        tween.TweenCallback(Callable.From(() => label.Text = Lines[line]));
        tween.TweenProperty(label, "modulate", new Color(label.Modulate, 1), 1);
    }
    public override void _Input(InputEvent @event)
    { 
        if (tween.IsRunning ())
        {
            return;
        }
        if (@event is InputEventKey key && key.Pressed && !key.Echo ||
        @event is InputEventMouseButton mouse && mouse.Pressed ||
        @event is InputEventJoypadButton joy && joy.Pressed)
        {
            if (currentline+1<Lines.Length)
            {
                ShowLine(currentline + 1);
            }
            else
            {
                var game = GetNode<Game>("/root/Game");
                game.BackToTitle();
            }
        }
    }
    public override void _Ready()
	{
		ShowLine(0);
        GetNode<SoundManager>("/root/SoundManager").PlayBgm(bgm);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
