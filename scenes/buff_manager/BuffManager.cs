using System.Collections.Generic;
using Godot;

public partial class BuffManager : Node
{
    public static BuffManager Instance {get; private set;}
    public  Dictionary<Buffs, int> currentBuffs = new Dictionary<Buffs, int>();

    public override void _EnterTree()
    {
        Instance = this;
        FillDefaultBuffs();
    }

    public enum Buffs
    {
        POTION_POTENCY,
        AMMO_MULT_MODIFIER,
        MAX_HEALTH
    }

    public void FillDefaultBuffs()
    {
        currentBuffs.Add(Buffs.POTION_POTENCY, 10);
        currentBuffs.Add(Buffs.AMMO_MULT_MODIFIER, 1);
        currentBuffs.Add(Buffs.MAX_HEALTH, 100);
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
