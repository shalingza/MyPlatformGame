using Godot;
using System;
using System.Threading.Tasks;

public partial class Hitbox : Area2D
{
    [Signal]
    public delegate void HitEventHandler(Hurtbox  hurtbox);


	
	void OnAreaEntered(Area2D hurtbox)
	{
        GD.Print($"{Owner.Name} => {hurtbox.Owner.Name}");

        EmitSignal("Hit", hurtbox );//发送攻击信号 参数为hurtbox 表示谁进行的攻击
        hurtbox.EmitSignal("Hurt", this);//将攻击者自身让hurtbox发射出去，作为收到攻击的信号，表示收到了谁的伤害
        
    }

    async void  OnHitboxHit(Hurtbox  Hurtbox)
    {
        
        var game = GetNode<Game>("/root/Game");
        game.ShakeCamera(2);

        Engine.TimeScale = 0.1f;
        await ToSignal(GetTree().CreateTimer(0.1f, true, false , true), Timer.SignalName.Timeout);
        Engine.TimeScale = 1f;
        
    }

    public override void _Ready()
	{
        AreaEntered += OnAreaEntered;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
