using Godot;
using System;

[GlobalClass]
public partial class StateMachine : Node
{
    private int _currentState = -1;
    public float statetime;

    public int CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;

            // 通用调用：通过 Owner 动态调用方法
            Owner.Call("TransitionState", _currentState, value);
            _currentState = value;
            statetime = 0;
        }
    }

    public override async void _Ready()
    {
        await ToSignal(Owner, Node.SignalName.Ready);
        _currentState = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Owner == null) return;

        statetime += (float)delta;

        // 通用调用：获取下一个状态
        int nextState = Owner.Call("GetNextState", _currentState).AsInt32();

        if (nextState != _currentState)
        {
            CurrentState = nextState;
        }

        // 通用调用：执行物理更新
        Owner.Call("TickPhysics", _currentState, (float)delta);
    }
}