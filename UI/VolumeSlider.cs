using Godot;
using System;

public partial class VolumeSlider : HSlider
{
	[Export]StringName bus = "Master";
	int busindex ;
    

    public override void _Ready()
	{
        busindex = AudioServer.GetBusIndex(bus);
        var sound = GetNode<SoundManager>("/root/SoundManager");
        var game = GetNode<Game>("/root/Game");
        Value = sound.GetVolume(busindex );
        ValueChanged += (double v) =>
        {
            sound.SetVolume(busindex, (float)v);
            game.SaveConfig();
        };
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
