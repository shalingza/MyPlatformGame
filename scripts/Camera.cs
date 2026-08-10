using Godot;
using System;

public partial class Camera : Camera2D
{
    [Export] TileMapLayer tilemap;
    [Export] Camera2D camera;

    public override void _Ready()
    {
        var used = tilemap.GetUsedRect().Grow(-1);
        var tilesize = tilemap.TileSet.TileSize;

        camera.LimitTop = used.Position.Y * tilesize.Y;
        camera.LimitRight = used.End.X * tilesize.X;
        camera.LimitLeft = used.Position.X * tilesize.X;
          
        camera.LimitBottom = used.End.Y * tilesize.Y;
    
    }
}