using Godot;
using System;

public partial class ShakyCamera : Camera2D
{
	[Export ]float strength = 0.0f;
	[Export] int recoveryspeed = 16;
    
    public override void _Ready()
	{
        var game = GetNode<Game>("/root/Game");
        game.CameraShake += (float amount) => strength += amount;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        float randomFloat = (float)GD.Randf() * 2f * strength - strength;
        Offset  = new Vector2(randomFloat, randomFloat);
		strength = Mathf.MoveToward(strength ,0,recoveryspeed *(float )delta);

    }
}
