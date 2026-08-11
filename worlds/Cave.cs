using Godot;
using System;
using System.Threading.Tasks;

public partial class Cave : World
{
    async void OnBoarDied()
    {
        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);
        var game = GetNode<Game>("/root/Game");
        await game.ChangeScene("res://UI/gameendscreen.tscn", new Godot.Collections.Dictionary(){
    { "duration", 1f }
     });
    }
    public override void _Ready()
    {
        base._Ready();

    }

}
