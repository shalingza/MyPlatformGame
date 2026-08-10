using Godot;
using System;

public partial class StatusPanel : HBoxContainer
{
	[Export] TextureProgressBar Healthbar;
	[Export] Stats stats;
	[Export] TextureProgressBar easehealthbar;
	[Export] TextureProgressBar Energybar;

	void UpdateHealth(bool skipanimation = false)
	{
        if (Healthbar == null || !IsInsideTree()) return;
		
        var percentage = stats.health / (float)stats.maxhealth;
        Healthbar.Value = percentage;
		if (skipanimation)
		{
			easehealthbar.Value = percentage;
		}
		else 
		{
			CreateTween().TweenProperty(easehealthbar ,"value",percentage ,0.3);
		}
        GD.Print($"血量: {stats.health}, 百分比: {percentage}, 血条值: {Healthbar.Value}");
		
		
    }

	void UpdateEnergy()
	{
        if (Energybar == null || !IsInsideTree()) return;
        var percentage = stats.energy / (float)stats.maxenergy;
		Energybar.Value = percentage;
	}

	public override void _Ready()
	{   
		if (stats == null)
		{
            stats = GetNode<Game>("/root/Game").playerstats;
        }
		stats.Healthchange += UpdateHealth;
		stats.Energychange += UpdateEnergy;
        GD.Print("信号连接成功");
        UpdateHealth(true);
		UpdateEnergy();
		
		
	}
    public override void _ExitTree()
    {
        if (stats != null)
        {
            stats.Healthchange -= UpdateHealth;
            stats.Energychange -= UpdateEnergy;
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
