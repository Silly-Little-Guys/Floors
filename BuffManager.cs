using System.Collections.Generic;
using Godot;

public partial class BuffManager : Node
{
    public static BuffManager Instance {get; private set;}
    public  Dictionary<Buffs, int> currentBuffs {get; private set;} = new Dictionary<Buffs, int>();

    public override void _EnterTree()
    {
        Instance = this;
        fillDefaultBuffs();
    }

    public enum Buffs
    {
        POTION_POTENCY,
        AMMO_MULT_MODIFIER,
        MAX_HEALTH
    }

    public void fillDefaultBuffs()
    {
        currentBuffs.Add(Buffs.POTION_POTENCY, 10);
        currentBuffs.Add(Buffs.AMMO_MULT_MODIFIER, 1);
        currentBuffs.Add(Buffs.MAX_HEALTH, 100);
    }
}
