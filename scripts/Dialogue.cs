using Godot;
using System;
[GlobalClass]
public partial class Dialogue : Resource
{
    [Export]public  string charactername;
    [Export(PropertyHint.MultilineText)]public string content;
    [Export]public  Texture avatar;
    [Export]public  bool showonleft;
   

}
