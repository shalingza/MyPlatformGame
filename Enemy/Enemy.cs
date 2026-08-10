using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]public  Node2D graphics;//控制翻转
	[Export]public  AnimationPlayer animationPlayer;//控制动画
	[Export]public  Node statemachine;//控制状态机转换

	[Export] public  float maxspeed = 180.0f;
	[Export] public  float acceleration = 2000;
    public  float defaultgravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
    [Signal] public delegate void DiedEventHandler();
    public enum State
    {
        IDLE,
        WALK,
        RUN,
		HURT,
		DYING,

    }
    public  enum Direction
	{
		Left = -1,
		Right = +1,

	}
      public Direction _direction = Direction.Left;
    [Export]public  Direction direction
	{
        get => _direction;
        set
		{
			
			_direction = value;
			graphics.Scale =new Vector2 ( -(int)direction ,graphics .Scale .Y );
		} 
	
	
	}   

	public  void Move(float speed ,float delta )
	{

        Velocity = new Vector2(Mathf.MoveToward(Velocity.X, (int)direction * speed, acceleration * (float)delta), Velocity.Y);
        Velocity += new Vector2(0, defaultgravity * (float)delta);
        MoveAndSlide();
    }
	public void Die()
	{
	   EmitSignal("Died"); 
	   QueueFree();
	}


    public override void _Ready()
	{
		AddToGroup("enemies");
	}

	
	public override void _Process(double delta)
	{
	}
}
