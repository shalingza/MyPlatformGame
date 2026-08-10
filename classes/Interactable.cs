using Godot;
using System;

public partial class Interactable : Area2D
{



    [Signal] public delegate void InteractedEventHandler();
   public Interactable()
	{
		CollisionLayer = 0;
		CollisionMask = 0;
		SetCollisionMaskValue(2,true);

		BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

	public virtual void Interact()
	{
        EmitSignal(SignalName.Interacted );
        GD.Print($"交互触发: {Name}");
    }
	void OnBodyEntered(Node body)
	{   

		if (body is Player player)
		{
           player.RegisterInteractable (this);
		}

		
	}
	void OnBodyExited(Node body)
	{
		if (body is Player player)
		{
			player.UnregisterInteractable(this);
		}
	}

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
