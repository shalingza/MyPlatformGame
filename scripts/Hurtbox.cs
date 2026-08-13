using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class Hurtbox : Area2D
{
	[Export ] Stats stats;
    [Signal]
    public delegate void HurtEventHandler(Hitbox hitbox);


	void OnHurtboxHurt(Hitbox hitbox)
	{
        if (Owner is Npc npc)
        {
            
            npc.OnHurtByPlayer(hitbox);// 触发 NPC 的反击（可以用信号或直接调用方法）
            return;
        }
        if (stats.health <= 0) return;
        Damage pendingdamage = new Damage();
		pendingdamage.amount = 1;
		pendingdamage.source = (Node2D)hitbox.Owner;
        GD.Print($"source 是: {pendingdamage.source}");
        Owner.Set("Pendingdamage", pendingdamage);
    }



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
      
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
