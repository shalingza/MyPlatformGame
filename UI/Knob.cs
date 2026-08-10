using Godot;
using System;

public partial class Knob : TouchScreenButton
{
	int fingerindex = -1;//有几根手指在屏幕上
	const int dragradius = 50;
	Vector2 restpos;
	Vector2 dragoffset;
    public override void _Input(InputEvent @event)
    {
		if (@event is InputEventScreenTouch touch)
		{
			if (touch.Pressed &&fingerindex ==-1)//检测触碰
			{   
				//按下
                var globalpos = GetCanvasTransform().AffineInverse() * touch.Position;//从屏幕坐标转换成世界坐标
                var localpos = ToLocal(globalpos);  // 转换成局部坐标
                Rect2 rect = new Rect2(Vector2.Zero, TextureNormal.GetSize());
                if (rect.HasPoint(localpos))
                {
                    fingerindex = touch.Index;
					dragoffset = globalpos - GlobalPosition;
                }
            }
			else if (!touch .Pressed &&touch .Index ==fingerindex )//检测抬起
			{
				//松开
				Input.ActionRelease("moveleft");
                Input.ActionRelease("moveright");
                fingerindex = -1;
				GlobalPosition = restpos;
			}
		}
		else if (@event is InputEventScreenDrag drag)
		{
			if (drag.Index ==fingerindex )
			{
				//拖动
				var wishpos = drag.Position * GetCanvasTransform().AffineInverse() - dragoffset;
				var movement = (wishpos - restpos).LimitLength(dragradius);

                GlobalPosition = restpos +movement ;
				movement /= dragradius;//将移动映射成1和-1；
				if (movement .X >0)
				{
					Input.ActionPress("moveright",movement.X );
                    Input.ActionRelease("moveleft");
                }
				else if (movement .X <0)
				{
					Input.ActionPress("moveleft",-movement .X);
                    Input.ActionRelease("moveright");
                }

            }
		}
	}
	public override void _Ready()
	{
		restpos = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }
}
