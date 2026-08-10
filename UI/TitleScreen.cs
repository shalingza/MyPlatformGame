using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export] Button newgame;
    [Export] Button loadgame;
    [Export] AnimatedSprite2D animated;
    [Export] AudioStream bgm;
    public override void _Ready()
	{
       
        var game = GetNode<Game>("/root/Game");
        if (!game .HasSave ())
        {
            loadgame.Disabled = true;
            loadgame.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
        }
        GetNode<SoundManager>("/root/SoundManager").SetupUISound(this);

        newgame.GrabFocus();
        var buttons = GetTree().GetNodesInGroup("buttons");
        foreach (Node node in buttons)
        {
            if (node is Button btn)
            {
                btn.MouseEntered += () => btn.GrabFocus();
            }
        } 
        GetNode<SoundManager>("/root/SoundManager").PlayBgm(bgm);
    }


    async void OnNewGamePressed()
    {
        animated.Play("start");
       
        await ToSignal(animated, AnimatedSprite2D.SignalName.AnimationFinished);
        var game = GetNode<Game>("/root/Game");
            game.NewGame();

        
        
    }

    void OnLoadGamePressed()
    {
        var game = GetNode<Game>("/root/Game");
        
        
        game.LoadGame();
    }
    void OnExitGamePressed()
    {
        GetTree().Quit();
    }

    public override void _Process(double delta)
	{
       
    }
}
