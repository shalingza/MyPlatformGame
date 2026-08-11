using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;

public partial class Game : Node
{
    [Export] public Stats playerstats;//玩家状态
    [Export] public ColorRect colorrect;//转场的颜色块
    Godot.Collections.Dictionary worldstates = new Godot.Collections.Dictionary();//存储敌人存活和敌人的路径
    const string savepath = "user://data.sav";//玩家游戏内容数据
    const string saveconfigpath = "user://config.ini";//存储玩家设置数据
    Godot.Collections.Dictionary defaultplayerstats;
    [Signal]
    public delegate void CameraShakeEventHandler(float amout);


    public  void ShakeCamera(float amout)
    {
        EmitSignal("CameraShake", amout);

    }
    public async Task ChangeScene(String path, Godot.Collections.Dictionary param = null  )//场景跳转函数
	{
        if (!ResourceLoader.Exists(path))
        {
            GD.PrintErr("路径不存在：" + path);
            return;
        }
        if (param == null)
        {
            param = new Godot.Collections.Dictionary();
        }
        float duration = param.ContainsKey("duration") ? (float)param["duration"] : 0.2f;
        var tree = GetTree();//获取当前根节点
        tree.Paused = true;//暂停

        var tween = CreateTween();//创建转场效果对象
        tween.SetPauseMode(Tween.TweenPauseMode.Process);//使转场效果在暂停时仍然可以行动
        tween.TweenProperty(colorrect , "color", new Color(colorrect.Color, 1), duration );//控制颜色变化
        await ToSignal(tween, Tween.SignalName.Finished);//等待转场完成信号
        var oldname = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();//获取旧场景的名字
        if (tree.CurrentScene is World world)
        {
            worldstates[oldname] = world.ToDict();//将旧场景的敌人数据保存
        }


        tree.ChangeSceneToFile(path);//转场至目标名称场景
		await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);//等待转场完成
        var newname = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();//获取新场景的名字
        if (worldstates.ContainsKey(newname) && tree.CurrentScene is World newWorld)
        {
            var data = worldstates[newname].AsGodotDictionary();//创建数据字典
            if (data != null)
            {
                newWorld.FromDict(data);//读取存储的敌人数据字典内容
            }
        }
        var nodes = tree.GetNodesInGroup("entrypoints");//获取当前场景内的位置点的分组
        if (param.ContainsKey("entrypoint"))
        {
            string entrypoint = param["entrypoint"].ToString();
            foreach (Node node in nodes)//遍历寻找合适的入口
            {
                GD.Print($"节点: {node.Name}");
                if (node is EntryPoint entry && entry.Name == entrypoint)
                {
                    GD.Print($"找到匹配入口点: {entrypoint}");
                    tree.CurrentScene.Call("UpdatePlayer", entry.GlobalPosition, (int)entry.direction);//调用相应方法实现位置设定
                    var tween3 = CreateTween();
                    tween3.TweenProperty(colorrect, "color", new Color(colorrect.Color, 0), duration);
                    tree.Paused = false;
                    return;
                }
            }
        }
        if(param.ContainsKey("position")&& param.ContainsKey("direction"))
        {
            var posDict = param["position"].AsGodotDictionary();
            Vector2 pos = new Vector2(
                (float)posDict["x"],
                (float)posDict["y"]
            );
            tree.CurrentScene.Call("UpdatePlayer", pos, (int)param["direction"]);
        }
        tree.Paused = false;
        var tween2 = CreateTween();
        tween2.TweenProperty(colorrect, "color", new Color(colorrect.Color, 0), duration );
        
    }

   public  void SaveGame()
    {
        var scene = GetTree().CurrentScene;//获取当前根节点

        var scenename = scene.SceneFilePath.GetFile().GetBaseName();//获取场景名称
        Player player = null;

        if (scene is World world)
        {
            worldstates[scenename] = world.ToDict(); 
            player = world.GetNode<Player>("Player");
        }
        if (player == null)
        {
            GD.PrintErr("找不到玩家");
            return;
        }
        var data = new Godot.Collections.Dictionary();
        data["worldstates"] = worldstates;
        data["stats"] = playerstats.ToDict();
        data["scene"] = scene.SceneFilePath;
        data["player"] =
            new Godot.Collections.Dictionary
        {
                {"direction",player.direction},
                {"position",new Godot.Collections.Dictionary
                          {
                             { "x",player.GlobalPosition .X },
                             { "y",player.GlobalPosition .Y }
                          }


                   }
            };
        

        var json = Json.Stringify(data);
        
        using var file = FileAccess.Open(savepath, FileAccess.ModeFlags.Write);
        if (file ==null)
        {
            GD.PrintErr("无法保存游戏，文件打开失败");
            return;
        }
        file.StoreString(json);
        GD.Print("游戏已保存");
    }

    public async void LoadGame()
    {
        var file = FileAccess.Open(savepath ,FileAccess.ModeFlags .Read );//读取模式打开存储文件 
        if (file ==null )
        {
            return;
        }
        var json = file.GetAsText();
        var data= Json.ParseString (json ).AsGodotDictionary();
        if (data.TryGetValue("worldstates", out var wsVariant))
        {
            var wsDict = wsVariant.AsGodotDictionary();
            worldstates.Clear();
            foreach (var kvp in wsDict)
            {
                worldstates[kvp.Key] = kvp.Value;
            }
        }

       
        if (data.TryGetValue("stats", out var statsVariant)) // 恢复玩家数据
        {
            var statsDict = statsVariant.AsGodotDictionary();
            playerstats.FromDict(statsDict);
        }


        if (data.TryGetValue("scene", out var sceneValue))
        {
            string scenepath = sceneValue.ToString();
            if (data.TryGetValue("player", out var playerData))
            {
                var playerDict = playerData.AsGodotDictionary();
                var param = new Godot.Collections.Dictionary
            {
            {"direction",playerDict["direction"]},
            {"position",new Godot.Collections.Dictionary
                          {
                             { "x",playerDict["position"].AsGodotDictionary()["x"]},
                             { "y",playerDict["position"].AsGodotDictionary()["y"]}
                          }
                          },{ "duration", 1.5f }
              };
               await ChangeScene(scenepath, param);
            }
        }


        GD.Print("游戏已加载");
    }
    public async  void NewGame()
    {
        playerstats.FromDict(defaultplayerstats);

        // 清空世界状态
        worldstates.Clear();

        // 切换到初始场景
       await ChangeScene("res://worlds/Forest.tscn", new Godot.Collections.Dictionary
    {
        { "entrypoint", "StartPoint" },
        { "duration", 1f }
    });
    }



    public async void BackToTitle()
    {
        await ChangeScene("res://UI/title_screen.tscn", new Godot.Collections.Dictionary(){
    { "duration", 0.4f }
     }      );
    }


    public bool HasSave()
    {
        string fullPath = ProjectSettings.GlobalizePath(savepath);
        GD.Print($"查找路径: {fullPath}");
        GD.Print($"存档文件存在, 路径: {savepath}");
        return FileAccess.FileExists(savepath);
    }

    public void SaveConfig()
    {
        ConfigFile config = new ConfigFile();
        var sound = GetNode<SoundManager>("/root/SoundManager");
        config.SetValue("audio","master",sound.GetVolume((int)SoundManager.Bus.Master));
        config.SetValue("audio", "sfx", sound.GetVolume((int)SoundManager.Bus.Sfx));
        config.SetValue("audio", "bgm", sound.GetVolume((int)SoundManager.Bus.Bgm));

        config.Save(saveconfigpath);
    }
    public void LoadConfig()
    {
        ConfigFile config = new ConfigFile();
        config.Load(saveconfigpath);
        var sound = GetNode<SoundManager>("/root/SoundManager");
        sound.SetVolume
        (   (int)SoundManager.Bus.Master ,
            (float)config.GetValue("audio","master",0.5)
        );
        sound.SetVolume
        ((int)SoundManager.Bus.Sfx ,
            (float)config.GetValue("audio", "sfx", 1)
        );
        sound.SetVolume
        ((int)SoundManager.Bus.Bgm ,
            (float)config.GetValue("audio", "bgm", 1)
        );



    }


    public override void _Ready()
	{
        colorrect.Color = new Color(colorrect.Color, 0);
        defaultplayerstats = playerstats.ToDict();
        LoadConfig();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
