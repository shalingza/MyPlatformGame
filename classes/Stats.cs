using Godot;
using System;
using System.Collections.Generic;

public partial class Stats : Node
{

    [Signal]public delegate void HealthchangeEventHandler(bool skipanimation);
    [Signal]public delegate void EnergychangeEventHandler();
    [Export]public  int maxhealth = 3;
    private int _health;
   [Export] public int health
	{
		get => _health;
		set
		{
            int newHealth = Math.Clamp(value ,0,maxhealth);
			if (_health ==newHealth  )
			{
				return;
			}
			else
			{
				_health =newHealth; 
				GD.Print($"血量变化: {_health}/{maxhealth}");
                EmitSignal(SignalName.Healthchange,false);
			}
           

        }



	}
    [Export] public float  maxenergy = 10f;
    private float  _energy;
    private float energyregen = 1f;
    [Export]
    public float energy
    {
        get => _energy;
        set
        {
            float newEnergy = Math.Clamp(value, 0f, maxenergy);
            if (_energy == newEnergy)
            {
                return;
            }
            else
            {
                _energy = newEnergy ;
                
                EmitSignal(SignalName.Energychange);
            }


        }



    }
    public Godot.Collections.Dictionary ToDict()
    {

        return new Godot.Collections.Dictionary//把列表和名称按字典的形式返回回去
        {
          { "maxenergy",maxenergy  },
          { "maxhealth",maxhealth },
          {"health",health }
        };
    }

    public void FromDict(Godot.Collections.Dictionary data)
    {
        if (data.TryGetValue("maxenergy", out var value))
        {
            maxenergy = (float)value;
        }
        if (data.TryGetValue("maxhealth", out var value2))
        {
            maxhealth = (int)value2;
        }
        if (data.TryGetValue("health", out var value3))
        {
            health = (int)value3;
        }


    }




    public override void _Ready()
	{
		 health = maxhealth;
         energy = maxenergy;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        energy += energyregen * (float )delta;
	}
}
