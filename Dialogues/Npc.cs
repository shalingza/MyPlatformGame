using Godot;
using System;

public partial class Npc : Interactable
{
    [Export] public DialogueGroup[] dialoguestages;  // 对话数据组
    private int currentstage = 0;//对话标
    private int talkCount = 0; // 当前对话次数
    private int attackedTalkCount = 0;// 被攻击后的对话次数
    private bool hasbeenattacked = false;
    private Player player;
    [Export] public Node2D Graphics;
    private bool isCounterAttacking = false;
    [Export] AnimationPlayer animationplayer;//动画控制器
    public override void Interact()
    {
        base.Interact();
        var dialogueManager = GetNode<DialogueManager>("/root/UIManager/DialogueUI");
        if (dialogueManager == null) return; ;
        talkCount++; // 对话次数+1
        DialogueGroup targetdialogue = null;
        if (hasbeenattacked)
        {
            // 被攻击后，每次对话累计被攻击后的对话次数
            attackedTalkCount++;

            if (attackedTalkCount >= 3 && dialoguestages.Length > 2)
            {
                targetdialogue = dialoguestages[2]; // 第三阶段（索引2）
            }
            else if (dialoguestages.Length > 1)
            {
                targetdialogue = dialoguestages[1]; // 第二阶段（索引1）
            }

        } 
         else if (talkCount >= 3 && dialoguestages.Length > 3)
        {
            targetdialogue = dialoguestages[3];//正常对话3次以上
        }
        else
        {
            targetdialogue = dialoguestages[0]; // 默认  第一阶段
        }
        

        dialogueManager.SetPlayer(player);
        dialogueManager.StartDialogue(targetdialogue);
    }

     void FlipTowardPlayer()
    {
        if (player == null || Graphics  == null) return;

        // 比较 X 坐标
        if (player.GlobalPosition.X < GlobalPosition.X)
        {
            // 玩家在左边 → 朝左
            Graphics .Scale = new Vector2(-1, 1);
        }
        else if (player.GlobalPosition.X > GlobalPosition.X)
        {
            // 玩家在右边 → 朝右
             Graphics.Scale = new Vector2(1, 1);
        }
        // 如果 X 相等，保持当前朝向
    }
    public void OnHurtByPlayer(Hitbox hitbox)
    {
        
        if (isCounterAttacking || player == null) return;

        isCounterAttacking = true;
         hasbeenattacked = true;
         // 创建伤害对象
        Damage damage = new Damage();
        damage.amount = 2;
        damage.source = this;  // 伤害来源是 NPC 自身

        // 赋值给玩家，触发受伤
        player.Pendingdamage = damage;
        animationplayer.Play("attack");
        GetNode<SoundManager>("/root/SoundManager").PlaySFX("Npcattack");
        // 延迟恢复，防止连续触发
        GetTree().CreateTimer(0.3f).Timeout += () => isCounterAttacking = false;
    }
    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "attack")
        {
            animationplayer.Play("idle");
        }
    }
    public override void _Ready()
	{
        player = GetNode<Player>("/root/World/Player");
        if (player == null)
        {
            GD.PrintErr("找不到 Player 节点，朝向功能将失效");
        }
        var hurtbox = GetNode<Hurtbox>("Graphics/Hurtbox");
        if (hurtbox != null)
        {
            hurtbox.Hurt += OnHurtByPlayer;
        }
        if (animationplayer != null)
        {
            animationplayer.AnimationFinished += OnAnimationFinished;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        FlipTowardPlayer();
    }
}
