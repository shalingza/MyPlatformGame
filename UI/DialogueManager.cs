using Godot;
using System;

public partial class DialogueManager : Control
{
	[ExportGroup ("UI")]
	[Export] Label characternametext;
	[Export] Label textbox;
	[Export] TextureRect leftavatar;
	[Export] TextureRect rightavatar;
    [ExportGroup("对话")]
    [Export] DialogueGroup maindialogue;
	int dialogueindex = 0;
	Tween typingtween;
    bool isTyping = false;
    private Player _player;
    void DisplayNextDialogue()
	{
       
        if (dialogueindex >= maindialogue .dialoguelist .Length )
		{
			Visible = false;
			return;
		}
        // 如果正在打字，立即完成当前对话并进入下一句
        if (isTyping)
        {
            typingtween?.Kill(); // 停止当前打字动画
            // 直接显示完整内容
            var current = maindialogue.dialoguelist[dialogueindex];
            textbox.Text = current.content;
            isTyping = false;
            dialogueindex++;
            return;
        }




        var dialogue = maindialogue.dialoguelist[dialogueindex];
		characternametext.Text = dialogue.charactername;
		typingtween = GetTree().CreateTween();
		textbox.Text = "";
        isTyping = true;
        string fullText = dialogue.content;

        
        typingtween.TweenMethod(Callable.From<float>(value =>
        {
            int charCount = Mathf.RoundToInt(value);
            textbox.Text = fullText.Substring(0, charCount);// 截取前 charCount 个字符
        }), 0f, fullText.Length,fullText.Length/10); //实现动态控制打字机速度

        if (dialogue.showonleft)
		{
			leftavatar.Texture = (Texture2D)dialogue.avatar;
			rightavatar.Texture = null;
		}
		else
		{
			leftavatar.Texture = null;
			rightavatar .Texture = (Texture2D)dialogue.avatar;
        }
        typingtween.TweenCallback(Callable.From(() =>
        {
            isTyping = false;
            dialogueindex++;

        })); // 不自动进入下一句，等待用户点击
    }
    public void StartDialogue(DialogueGroup group)
    {
        
        Visible = true;
        if (_player != null)
        {
            _player.isindialogue = true;
            _player.statemachine.CurrentState = (int)Player.state.Dialogue;
        }
        
        if (group == null || group.dialoguelist == null || group.dialoguelist.Length == 0)
        {
            GD.PrintErr("对话数据为空");
            return;
        }
       
        maindialogue = group;
        dialogueindex = 0;
       
        DisplayNextDialogue();
    }



     public override void _Input(InputEvent @event)
    {
        if (!Visible) return;
        if ((@event is InputEventKey key && key.Pressed && !key.Echo && (key.Keycode == Key.E || key.Keycode == Key.K)) ||
        (@event is InputEventMouseButton mouse && mouse.Pressed) ||
        (@event is InputEventJoypadButton joy && joy.Pressed))
        {
            DisplayNextDialogue();
            GetWindow().SetInputAsHandled();
        }


    }
    private void UnlockPlayer()
    {
        if (_player != null)
        {
            _player.isindialogue = false;
            _player.statemachine.CurrentState = (int)Player.state.Idle;
        }
    }
    public void SetPlayer(Player player)
    {
        
        _player = player;
        if (_player != null)
        {
            GD.Print("DialogueManager: Player 引用已设置");
        }
    }
    public override void _Ready()
	{
        
        Visible = false  ;
        VisibilityChanged += () =>
        {
            if (!Visible && _player != null)
            {
                _player.isindialogue = false;
                _player.statemachine.CurrentState = (int)Player.state.Idle;
            }
        };




    }
   
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
