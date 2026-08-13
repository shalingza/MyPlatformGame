using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Enemy;
using static Godot.TextServer;
using static System.Net.Mime.MediaTypeNames;

public partial class Player : CharacterBody2D
{

   public  enum Direction
    {
      Left=-1,

      Right=+1,

    }



    public enum state //状态总类
    {
      Idle,
      Running,
      Jump,
      Fall,
      Landing,
      Wallsliding,
      Walljump,
      Attack1,
      Attack2,
      Attack3,
      Hurt,
      Dying,
      Slidingstart,
      Slidingloop,
      Slidingend,
      Dialogue,
    }
    
    const int knockbackamount = 312;//击退力度
    const float slidingduration= 0.3f;//冲刺时间
    const float slidingspeed = 256.0f;//冲刺速度
    const float landingheight = 100.0f;//跳下来触发着陆动画的像素高度
    const float slidingenergy = 4f;//滑铲的能量

    public bool isindialogue { get; set; } = false;
    bool isfirsttick = false;//用来判断是否是刚进入该状态的第一帧
    private state[] groundStates = [state.Idle, state.Running,state.Landing ,state.Attack1 ,state.Attack2 ,state .Attack3];//判断哪些状态是处于在地板上的状态
    float defaultgravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");//玩家受到的重力效果
    float acceleration;//当前加速度

    public List<Interactable> interactingwith = new List<Interactable>(); //交互控制器
    public Damage Pendingdamage=null ;
    bool canjump;//是否能跳跃
    public float fallfrom;//表示角色从多高跳下来
    bool iscombolrequest = false;//判断连击是否发生 
    bool hasJumped = false;  // 是否已经跳过了
    bool wasonfloor =false ;//是否已经离开了地板
    Stats stats;//血量体力状态控制器
    [Export] public StateMachine statemachine;//状态机
    [Export] Timer coyotetimer;//离开地面后可跳跃的倒计时
    [Export] Timer jumprequestimer;//跳跃预输入的时间
    [Export] Timer sliderequestimer;//滑铲预输入的时间
    [Export] Timer invinvibletimer;//玩家受击无敌的时间
    [Export] Timer comboltimer;//连击倒计时
    [Export] RayCast2D handchecker;//手部的射线检测用来测试爬墙
    [Export] RayCast2D footchecker;//脚部的射线检测同样用来测试爬墙
    [Export] float speed = 150f;//移动速度
    [Export] float jumpVelocity = -400f;//跳跃速度
    [Export] Vector2 walljumpvelocity = new Vector2(400,-320);//蹬墙跳跃的速度
	[Export] AnimationPlayer animationplayer;//玩家动画控制器
    [Export] Node2D graphics;//控制翻转	
    [Export] bool cancombol = false;//是否可以连击
    [Export] AnimatedSprite2D interactionicon;//交互动画
    [Export] int _direction =(int)Direction.Right;//设置方向
    [Export] GameOverScreen gameoverscreen;//游戏结束页面
    [Export] Control pausescreen;//暂停界面
    public int direction
    {
        get => _direction;
        set
        {
            _direction = value;
            if (IsNodeReady())
            {
               ApplyDirection(value);
            }
            
        }
        
    }
    private void ApplyDirection(int value)
    {
        graphics.Scale = new Vector2(value, graphics.Scale.Y);
    }
    bool CanWallSlide()//判断是否抓住墙体的函数
    {
        return IsOnWall() && handchecker.IsColliding() && footchecker.IsColliding();
    }
    
    public override void _UnhandledInput(InputEvent @event)//控制跳跃的函数
    {
        if (isindialogue) return;
        if (Input.IsActionJustPressed("pause"))
        {
            var pausescreen = GetNode<PauseScreen>("CanvasLayer/PauseScreen");
            if (pausescreen != null)
            {
                pausescreen.ShowPause();
            }
        }
        
        
        
        

        if (Input.IsActionJustPressed("jump"))
        {
            jumprequestimer .Start();
           
        }
        if (Input .IsActionJustReleased ("jump"))
        {
            jumprequestimer.Stop();
            if (Velocity.Y < jumpVelocity / 2)
            {
               Velocity =  new Vector2(Velocity .X ,jumpVelocity / 2);
            }
            
        }
        if (Input .IsActionJustPressed ("attack")&&cancombol)
        {
            iscombolrequest = true;
            comboltimer.Start();
        }
        if (Input .IsActionJustPressed ("slide"))
        {
            sliderequestimer.Start();
        }
        if (Input.IsActionJustPressed("interact")&&interactingwith !=null && interactingwith.Count > 0)
        {
            interactingwith[interactingwith.Count - 1]. Interact();
        }
        
    }

    public void TickPhysics(state State, float delta)//用来判断当前的状态下可以做的活动
    {
        if (isindialogue) return;
        interactionicon.Visible = interactingwith != null&& interactingwith.Count > 0;
        if (invinvibletimer .TimeLeft >0)//无敌时闪光且无视怪物碰撞
        {
            graphics.Modulate = new Color(graphics.Modulate,MathF.Sin((float)Time.GetTicksMsec()/30f)*0.5f+0.5f );
            CollisionLayer = 0;
            CollisionMask = 1;
        }
        else
        {
            graphics.Modulate = new Color(graphics.Modulate, 1);
            CollisionLayer = 2;
            CollisionMask = 1;
        }

        switch (State)
        {
            case state.Idle:
                move(defaultgravity ,delta);
                break;
            case state.Running:
                move(defaultgravity,delta);
                break;
            case state.Jump:
                if (isfirsttick )
                {

                    move(0.0f, delta);
                }
                else
                {

                    move(defaultgravity, delta);
                }
                break;
            case state.Fall:
                move(defaultgravity, delta);
                break;
            case state.Landing:
                move(defaultgravity, delta);
                break;
            case state.Wallsliding:
                move(defaultgravity/4.5f, delta);
                direction = (int)GetWallNormal().X;
                if (Velocity.Y < 0)
                {
                    Velocity = new Vector2(Velocity.X, 0);
                }
                if (Velocity.Y > 150)  // 最大下落速度
                {
                    Velocity = new Vector2(Velocity.X, 150);
                }
                break;
            case state.Walljump:
                if(statemachine.statetime >0)
                {
                   if (isfirsttick)
                   {

                    move(0.0f, delta);
                   }
                   else
                   {
 
                    move(defaultgravity, delta);
                   }
                    direction = (int)GetWallNormal().X;
                }
                break;
            case state.Attack1:
                attack(delta );
                    break;
            case state.Attack2:
                attack(delta);
                break;
            case state.Attack3:
                attack(delta);
                break;
            case state.Hurt :
                Velocity = new Vector2(Velocity.X, Velocity.Y + defaultgravity * delta);
                MoveAndSlide();
                
                break;
            case state.Dying :
                Velocity = Vector2.Zero;
                MoveAndSlide();
                break;
            case state.Slidingend:
                Velocity = Vector2.Zero;
                MoveAndSlide();
                break;
            case state.Slidingstart:
                slide(delta);
                break;
            case state.Slidingloop:
                slide(delta);
                break;
            case state.Dialogue:
                MoveAndSlide();
                break;
        }

        isfirsttick = false;


    }
    bool Shouldslide()
    {
        if (sliderequestimer .IsStopped ()||footchecker .IsColliding ())
        {
            return false;
        }
        if (stats.energy < slidingenergy)
        {
            return false;
        }
        return true;
    }
    void slide(float delta)
    {
        Velocity = new Vector2(graphics .Scale .X *slidingspeed ,Velocity .Y+defaultgravity *delta );

        MoveAndSlide();
    }

    void attack(float delta)
    {
        // 攻击时站立不动（速度为0）
        Velocity = new Vector2(0, Velocity.Y);

        // 可以转向（在 move 函数中已经处理了方向，这里只需要确保 graphics.Scale 正确）
        var movement = Input.GetAxis("moveleft", "moveright");
        if (movement != 0)
        {
            direction = movement < 0 ? (int)Direction.Left : (int)Direction.Right;
        }

        // 应用重力（保持重力效果）
        Velocity += new Vector2(0, defaultgravity * delta);
        MoveAndSlide();
    }

    void move(float gravity ,float delta)//用来控制各种移动的函数 
    { 
        float flooracceleration = speed / 0.2f;//主角的加速度--逐渐加速
        float airacceleration = speed / 0.1f;//主角在空中的加速度--空气阻力
        
        if (IsOnFloor())
        {
            acceleration = flooracceleration;
           
            
        }
        else
        {
            acceleration = airacceleration;
        }
        if (IsOnFloor ())
        {   
            hasJumped = false;
            canjump = true;
            coyotetimer.Stop();  // 在地板上，停止计时器
        }
        else if (coyotetimer.TimeLeft > 0)
        {
            canjump = true; 
        }
        else
        {
            canjump = false;  
        }
        var movement = Input.GetAxis("moveleft","moveright");
        Velocity=new Vector2 ( Mathf.MoveToward(Velocity .X ,movement *speed ,acceleration *(float )delta),Velocity.Y);
        Velocity += new Vector2(0, gravity * (float)delta);

      





        if (movement != 0)
        {   

            if (  movement < 0)
            {
                direction = (int)Direction.Left;

            }
            else
            {
                direction = (int)Direction.Right ;
            }
           
        }

        bool isonfloor = IsOnFloor();
        if (!isonfloor && wasonfloor )
        {   
            coyotetimer.Start();
        }
        
        wasonfloor = isonfloor;
        MoveAndSlide();
        
    }

    public state GetNextState(state State)//得到下一个状态的函数 根据当前状态和判断用来确定下一个状态是什么
    {
        if (State == state.Dialogue) return state.Dialogue;

        if (stats.health == 0 && State != state.Dying)
        {

            return state .Dying ;
        }
        var movement = Input.GetAxis("moveleft", "moveright");
        var isstill = movement == 0&&Velocity.X ==0;
        if (jumprequestimer.TimeLeft > 0 && canjump && !hasJumped&&!(State == state.Dying))
        {
            return state.Jump;
        }
        
        if (Pendingdamage != null && State != state.Hurt && State != state.Dying)
        {

            return state.Hurt ;
        }

        switch (State)
        {
            case state.Idle:
                if (!IsOnFloor ())
                {
                    return state.Fall;
                }
                if (!isstill)
                    {
                        return state.Running;
                    }
                if (Input.IsActionJustPressed("attack"))
                {
                    return state.Attack1;
                }
                if (Shouldslide ())
                {
                    return state.Slidingstart;
                }
                    break;
                
                
            case state.Running:

                if (!IsOnFloor())
                {
                    return state.Fall;
                }
                if (Input.IsActionJustPressed("attack"))
                {
                    return state.Attack1;
                }
                if (isstill )
                {
                    return state.Idle;

                }
                if (Shouldslide ())
                {
                    return state.Slidingstart;
                }
                break;               
            case state.Jump:
                if (CanWallSlide () && !isfirsttick)
                {
                    return state.Wallsliding;
                }
                if (Velocity.Y > 0)
                {
                    return state.Fall;
                }
                    break;                
            case state.Fall:
                var height = GlobalPosition.Y - fallfrom;
                if (IsOnFloor ())
                { 
                    if (height>=landingheight )
                    {
                        return state.Landing; 
                    }
                    else
                    {
                        return state.Running;
                    }  
                                   
                }
                if (CanWallSlide ())
                {

                    
                    return state.Wallsliding;

                }
                    break;
                
            case state.Landing:

                if (!animationplayer.IsPlaying())
                {
                    return state.Idle;
                    
                }
                    break;

            case state.Wallsliding:
                if (IsOnFloor ())
                {
                    return state.Idle;
                }
                if (!IsOnWall())
                {
                    return state.Fall;

                }
                if (jumprequestimer.TimeLeft > 0)
                {
                    return state.Walljump;
                }
                    break;
            case state.Walljump:
                if(CanWallSlide()&&!isfirsttick)
                {
                    return state.Wallsliding;
                }


                if (Velocity.Y >0)
                {
                    return state.Fall;
                }
                    break;
            case state.Attack1:
                if (!animationplayer .IsPlaying ())
                {  
                    if (iscombolrequest&&comboltimer.TimeLeft  > 0)
                    { 
                        return state.Attack2;
                    }
                    else 
                    {
                        return state.Idle;
                    }
                }

                break;
            case state.Attack2:
                if (!animationplayer.IsPlaying())
                {
                    if (iscombolrequest && comboltimer.TimeLeft > 0)
                    {
                        return state.Attack3;
                    }
                    else
                    {
                        return state.Idle;
                    }
                }

                break;
            case state.Attack3:
                if (!animationplayer.IsPlaying())
                {
                   
                    
                        return state.Idle;
                    
                }

                break;
            case state.Hurt :
                if (!animationplayer.IsPlaying())
                {


                    return state.Idle;

                }

                break;
            case state.Slidingstart :
                if (!animationplayer.IsPlaying())
                {


                    return state.Slidingloop ;

                }

                break;
            case state.Slidingloop:
                if (statemachine .statetime >slidingduration ||IsOnWall ()) 
                {
                    return state.Slidingend;
                }

                break;

                

            case state.Slidingend :
                if (!animationplayer.IsPlaying())
                {


                    return state.Idle;

                }

                break;

            
               

        }
        return State;
    }


    public  void TransitionState(state from, state to)//用来控制当前的状态转变后需要做的事情
    {

        if (!groundStates.Contains(from)&& groundStates.Contains(to))
        {
            coyotetimer.Stop();
        }

        switch (to)
        {
            case state.Idle:
                animationplayer.Play("idle");
                break;
            case state.Running:
                animationplayer.Play("running");
                break;
            case state.Jump:
                animationplayer.Play("jump");
                GetNode<SoundManager>("/root/SoundManager").PlaySFX("Jump");
                Velocity = new Vector2(Velocity.X, jumpVelocity);
                hasJumped = true;
                coyotetimer.Stop();
                jumprequestimer.Stop();
                break;
            case state.Fall:
                animationplayer.Play("fall");
                if (groundStates.Contains(from))
                {
                    coyotetimer.Start();
                }
                fallfrom = GlobalPosition.Y ;
                break;
            case state.Landing:

                animationplayer.Play("landing");


                break;
            case state.Wallsliding:
                animationplayer.Play("wallsliding");

                break;
            case state.Walljump:
                
                
                animationplayer.Play("jump");
                
                Velocity = new Vector2(GetWallNormal().X*walljumpvelocity .X  , walljumpvelocity .Y );
                hasJumped = true;
                
                jumprequestimer.Stop();
                
                break;
            case state.Attack1:
                animationplayer.Play("attack1");
                iscombolrequest = false;
                GetNode<SoundManager>("/root/SoundManager").PlaySFX("Attack1");

                break;
            case state.Attack2:
                animationplayer.Play("attack2");
                GetNode<SoundManager>("/root/SoundManager").PlaySFX("Attack2");
                iscombolrequest = false;
                break;
            case state.Attack3:
                animationplayer.Play("attack3");
                GetNode<SoundManager>("/root/SoundManager").PlaySFX("Attack3");
                iscombolrequest = false;
                break;
            case state.Hurt:
                animationplayer .Play("hurt");
                var game = GetNode<Game>("/root/Game");
                game.ShakeCamera(4);
                invinvibletimer.Start();
                stats.health -= Pendingdamage.amount;
                Vector2 hurtmovement = (GlobalPosition - Pendingdamage.source.GlobalPosition).Normalized();

                Velocity = hurtmovement * knockbackamount;
                


                Pendingdamage = null;
                break;


            case state.Dying:
                animationplayer.Play("die");
                invinvibletimer.Stop();
                CollisionLayer = 0;
                CollisionMask = 1;
                Pendingdamage = null;
                GetNode<SoundManager>("/root/SoundManager").PlaySFX("Death");
                interactingwith.Clear();
                break;
            case state.Slidingstart:
                animationplayer.Play("sliding_start");
                sliderequestimer.Stop();
                stats.energy -= slidingenergy;
                break;
            case state.Slidingloop:
                animationplayer.Play("sliding_loop");
                break;
            case state.Slidingend:
                animationplayer.Play("sliding_end");
                break;
            case state.Dialogue:
                animationplayer .Play ("idle");
                break;

        }
        isfirsttick = true;

    }

    void Die()
    {
        

        
        
        gameoverscreen.ShowGameOver();
    }


    void OnHurtboxHurt(Hitbox hitbox)
    {
        if (invinvibletimer.TimeLeft >0)
        {
            return;
        }
        Damage pendingdamage = new Damage();
        pendingdamage.amount = 1;
        pendingdamage.source = (Node2D)hitbox.Owner;
        GD.Print($"source 是: {pendingdamage.source}");
        Pendingdamage = pendingdamage;  // 直接赋值
    }

    public void RegisterInteractable(Interactable v)
    {   
        if (statemachine.CurrentState ==(int)state.Dying)
        {
            return;
        }
        if (interactingwith.Contains(v))
        {
            return;
        }
        interactingwith.Add(v);
    }
    public void UnregisterInteractable(Interactable v)
    {
        if (interactingwith.Contains(v))
        {
            interactingwith.Remove(v);
        }
    }



    public override void _Ready()
    {
        stats = GetNode<Stats>("/root/Game/Playerstats");
        ApplyDirection(direction);
    }
    

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
    }
}
