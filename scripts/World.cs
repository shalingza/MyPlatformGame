using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


public partial class World : Node2D
{
    [Export] TileMapLayer tilemap;//限制摄像头的地形图层
    [Export] Camera2D camera;
    [Export] Player  player;
    [Export] AudioStream bgm;
    public override void _Ready()
    {
        var used = tilemap.GetUsedRect().Grow(-1);
        var tilesize = tilemap.TileSet.TileSize;

        camera.LimitTop = used.Position.Y * tilesize.Y;
        camera.LimitRight = used.End.X * tilesize.X;
        camera.LimitLeft = used.Position.X * tilesize.X;

        camera.LimitBottom = used.End.Y * tilesize.Y;
        
        if (bgm!=null )
        {
            GetNode<SoundManager>("/root/SoundManager").PlayBgm(bgm);
        }
    }
    public  void UpdatePlayer(Vector2 pos,Player.Direction direction)
    {
        player.GlobalPosition = pos;//将玩家的位置放置在点上
        player.fallfrom = pos.Y;//将坠落高度和点平齐防止触发动画
        player.direction = (int)direction;//将玩家的方向设置成储存好的方向
        camera.ResetSmoothing();//平滑摄像头
        camera.ForceUpdateScroll();//强制更新一下摄像头

    }
     public Godot.Collections.Dictionary ToDict() 
    {
        var data = new Godot.Collections.Dictionary();
        var enemiesalive = new Godot.Collections.Array();
        foreach (Node node in GetTree().GetNodesInGroup ("enemies"))//将组内的元素便利并将其放入列表中
        {
            string path = GetPathTo(node);
            enemiesalive.Add(path);
        }
        data["enemiesalive"] = enemiesalive;
        return data;
    }
    public  void FromDict(Godot.Collections.Dictionary data)
    {
        if (!data.TryGetValue("enemiesalive", out var value)) return;//如果值是空的直接返回
        var enemiesalive = value.AsGodotArray();//把取出来的值重新变回列表
        if (enemiesalive == null) return;
        
        var alivelist = new List<string>();// 把 Array 转成 List<string>
        foreach (var item in enemiesalive)
        {
            alivelist.Add(item.ToString());
        }
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))//遍历列表 如果敌人不在里面 删掉
        {
            string path = GetPathTo(node);
            if (!enemiesalive.Contains(path))
            {
                node.QueueFree();
            }
        }
    }
    

}
