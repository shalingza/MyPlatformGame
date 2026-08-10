using Godot;
using System;

public partial class Boar : Enemy
{
	[Export] RayCast2D wallchecker;//墙壁检测 检测撞墙回头
	[Export] RayCast2D playerchecker;//玩家检测 检测进行攻击
	[Export] RayCast2D floorchecker;//地面检测 检测悬崖
    [Export] AnimationPlayer animationboar;//动画控制

    [Export] public StateMachine statemachine;//状态机类 用来控制状态机的运转
	[Export] Timer clamDownTimer;//狂暴状态下进入冷静状态所需要的时间 用来判断什么时候从奔跑状态进入走路状态或者静止状态
	[Export] Stats stats;//血量控制函数
	 Damage Pendingdamage=null;//表示待处理的伤害
	const int knockbackamount = 512;//击退力度
   public enum State 
	{
	  IDLE,
	  WALK,
	  RUN,
	  HURT,
	  DYING,
	}


	bool canseeplayer()
	{
        if (playerchecker.IsColliding())
        {
            var collider = playerchecker.GetCollider();
            if (collider is Node2D node && node.IsInGroup("player"))
            {
                return true ;  // 检测到玩家，进入奔跑状态
            }
        }
			return false;
		
    }

  


    public State GetNextState(State state)//获取下一个状态 并将该状态返回回去 在statemachine类中使用
	{
        

        if (stats.health ==0)
		{
            
            return State.DYING;
		}
        if (Pendingdamage !=null)
        {
           
            return State.HURT;
        }

        switch (state)
		{
			case State.IDLE:
                if (canseeplayer ())
                {
                    
                        return State.RUN;  // 检测到玩家，进入奔跑状态
                   



                }
                if (statemachine.statetime > 2)
				{
					return State.WALK;
				}
					break;
			case State.WALK:
                if (canseeplayer ())
                {
                    return State.RUN;  // 检测到玩家，进入奔跑状态
   
                }
                if (wallchecker .IsColliding ()||!floorchecker .IsColliding ())
				{
                    if (statemachine.statetime < 0.5f)
                    {
                        return State.WALK;  // 继续待在 WALK，不转身
                    }
                    return State.IDLE;

				}
					break;
			case State.RUN:
				if (!canseeplayer()&&clamDownTimer .IsStopped ())
				{
					return State.WALK;
				}


					break;
			case State.HURT:
				if (!animationboar.IsPlaying ())
				{
					return State.RUN;
				}
				break;





		}
		return state;


	}

    public void TransitionState(State from, State to)//用来控制当前的状态转变后需要做的事情
    {


		switch (to)
		{
			case State.IDLE:
				animationboar.Play("idle");
                if (wallchecker.IsColliding())
                {
                    direction = (Direction)(-(int)direction);



                }

                break;
				


			case State.WALK:
				animationboar.Play("walk");
				if (!floorchecker .IsColliding ())
                {
                    direction = (Direction)(-(int)direction);
                }
                break;

			case State.RUN:
				
				animationboar.Play("run");
				break;
			case State.HURT:
				animationboar.Play("hit");
                stats.health -= Pendingdamage.amount;
                Vector2 hurtdirection = (GlobalPosition - Pendingdamage.source.GlobalPosition).Normalized();

                Velocity = hurtdirection * knockbackamount;
				if (hurtdirection.X > 0)
				{
					direction = Direction.Left;

				}
				else
				{
					direction = Direction.Right;
				}


				Pendingdamage = null;
					break;
				
				
			case State.DYING:
				animationboar.Play("die");
				break;

		}
	}


	void TickPhysics(State state ,float delta)//根据当前的状态来控制行为的函数
	{
		switch (state)
		{
			case State.IDLE:
				Move(0,delta );
				break;
			case State.WALK:
               
                Move(maxspeed/3, delta);
				break;
			case State.RUN:
                if (wallchecker.IsColliding() || !floorchecker.IsColliding())
                {
                    direction = (Direction)(-(int)direction);
                }
				if (playerchecker .IsColliding ())
				{
                    var collider = playerchecker.GetCollider();
                    if (collider is Node2D node && node.IsInGroup("player"))
                    {
                      clamDownTimer.Start();
                    }
                    
				}
					Move(maxspeed, delta);

				break;
			case State.HURT:
                Move(0, delta);
				break;
			case State.DYING:
				Move(0, delta);
				break;

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
