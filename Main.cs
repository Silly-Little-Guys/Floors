using Godot;
using System;
using Godot.NativeInterop;

public partial class Main : Node2D
{
    // Called when the node enters the scene tree for the first time.
    private string directory = "res://Floors/";
    public static int amountOfFloors = 5;

    public override void _Ready()
    {
        String[] filelocations = DirAccess.GetFilesAt(directory);
        float yPos = 0;
        for (int i = 0; i < amountOfFloors; i++)
        {
            string filelocation = filelocations[GD.Randi() % (filelocations.Length)];
            Node2D floorNode = (Node2D)GD.Load<PackedScene>(directory + filelocation).Instantiate();
            var sprite2D = floorNode.GetNode<Sprite2D>("FloorBack");
            var height = sprite2D.Texture.GetHeight() * sprite2D.Scale.Y;
            
            var currentPos = floorNode.Position;
            floorNode.Position = new Vector2(currentPos.X, currentPos.Y - yPos);
            
            yPos += height;
            AddChild(floorNode);

            GD.Print(filelocation);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}