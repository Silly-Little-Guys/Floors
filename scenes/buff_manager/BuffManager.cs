using System.Collections.Generic;
using Godot;

public partial class BuffManager : Node
{
    public static BuffManager Instance { get; private set; }
    public Godot.Collections.Dictionary<Buffs, int> currentBuffs = new Godot.Collections.Dictionary<Buffs, int>();

    public override void _EnterTree()
    {
        Instance = this;
        LoadSave();
        GetTree().AutoAcceptQuit = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            SaveGame();
            GetTree().Quit();
        }
    }

    public void SaveGame()
    {
        EnsureDefaultBuffs();

        currentBuffs[Buffs.DEBUG] += 10;

        var saveData = new Godot.Collections.Dictionary<string, int>();

        foreach (Buffs buff in System.Enum.GetValues(typeof(Buffs)))
        {
            saveData[buff.ToString()] = currentBuffs[buff];
        }

        using FileAccess saveFile = FileAccess.Open("user://floors.save", FileAccess.ModeFlags.Write);
        saveFile.StoreLine(Json.Stringify(saveData));
    }
    private void EnsureDefaultBuffs()
    {
        if (!currentBuffs.ContainsKey(Buffs.POTION_POTENCY))
            currentBuffs[Buffs.POTION_POTENCY] = 10;

        if (!currentBuffs.ContainsKey(Buffs.AMMO_MULT_MODIFIER))
            currentBuffs[Buffs.AMMO_MULT_MODIFIER] = 1;

        if (!currentBuffs.ContainsKey(Buffs.MAX_HEALTH))
            currentBuffs[Buffs.MAX_HEALTH] = 100;

        if (!currentBuffs.ContainsKey(Buffs.DEBUG))
            currentBuffs[Buffs.DEBUG] = 0;
    }
    public void LoadSave()
    {
        FillDefaultBuffs();

        if (!FileAccess.FileExists("user://floors.save"))
        {
            GD.Print(currentBuffs);
            return;
        }

        using FileAccess saveFile = FileAccess.Open("user://floors.save", FileAccess.ModeFlags.Read);
        Variant parsed = Json.ParseString(saveFile.GetLine());

        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            GD.PrintErr("Save file is invalid. Using default buffs.");
            return;
        }

        var loadedData = parsed.AsGodotDictionary();

        foreach (Variant keyVariant in loadedData.Keys)
        {
            string key = keyVariant.AsString();

            if (!System.Enum.TryParse(key, out Buffs buff))
                continue;

            currentBuffs[buff] = (int)loadedData[key].AsDouble();
        }

        EnsureDefaultBuffs();

        GD.Print(currentBuffs);
    }

    public enum Buffs
    {
        POTION_POTENCY,
        AMMO_MULT_MODIFIER,
        MAX_HEALTH,
        DEBUG
    }

    public void FillDefaultBuffs()
    {
        currentBuffs.Clear();

        currentBuffs[Buffs.POTION_POTENCY] = 10;
        currentBuffs[Buffs.AMMO_MULT_MODIFIER] = 1;
        currentBuffs[Buffs.MAX_HEALTH] = 100;
        currentBuffs[Buffs.DEBUG] = 0;
    }

    public string GetBuffAsString(Buffs buff)
    {
        switch (buff)
        {
            case Buffs.POTION_POTENCY:
                return "Potion Potency";
            case Buffs.AMMO_MULT_MODIFIER:
                return "Ammo Multiplier";
            case Buffs.MAX_HEALTH:
                return "Max Health";
            default:
                return "cooked";
        }
    }
}
