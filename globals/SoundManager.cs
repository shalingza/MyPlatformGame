using Godot;
using System;

public partial class SoundManager : Node
{
	public enum Bus
    {
		Master,
		Sfx,
		Bgm,
		

	}




	[Export]Node sfx;
	[Export] AudioStreamPlayer bgmplayer;
    private bool uisoundenabled = false;
    public void PlayUISFX(string name)
    {
        if (!uisoundenabled) return;
        PlaySFX(name);
    }

    public void EnableUISound() => uisoundenabled = true;

    public void PlaySFX(String name)
	{
        AudioStreamPlayer player = sfx.GetNode<AudioStreamPlayer>(name);
		if (player==null)
		{
			return;
		}
		player.Play();
    }
	public void PlayBgm(AudioStream stream)

	{
		if (bgmplayer.Stream ==stream &&bgmplayer.Playing)
		{
			return;
		}
		bgmplayer.Stream = stream;
		bgmplayer.Play();
	}


	public void SetupUISound(Node node)
	{
		if (node is Button button)
		{
            button.Pressed += () => PlayUISFX("UIPress");
            button.FocusEntered += () => PlayUISFX("UIFocus");
            button.MouseEntered += () => button.GrabFocus();
        }
		if (node is Slider slider)
		{
            slider.ValueChanged += (double v) => PlayUISFX("UIPress");
            slider.FocusEntered += () => PlayUISFX("UIFocus");
            slider.MouseEntered += () => slider.GrabFocus();
        }
		foreach(var child in node.GetChildren())
		{
			SetupUISound(child);
		}
		
	}
	public float GetVolume(int busindex)//将分贝转化成线性数据
	{
		var db = AudioServer.GetBusVolumeDb(busindex);
		return Mathf.DbToLinear(db);
    }
	public void SetVolume(int busindex ,float v)//将数据转化回分贝并设置音量
	{
		var db = Mathf.LinearToDb(v);
		AudioServer.SetBusVolumeDb(busindex, db);
    }


	public override void _Ready()
	{
       
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
