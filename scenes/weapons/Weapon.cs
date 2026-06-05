using Godot;

public abstract partial class Weapon : Node2D
{
	[Signal] public delegate void OnAmmoCountUpdatedEventHandler(int ammo);
	[Export] public int damage;
	[Export] public float fireRate;
	[Export] public float bulletSpeed;
	[Export] public float spreadDegrees;
	[Export] public float airUpwardRecoilScale;
	[Export] public PackedScene bulletScene;
	[Export] public Timer fireTimer;
	[Export] public Node bulletSpawnPoint;
	[Export] public AnimatedSprite2D animatedSprite2D;
	[Export] public Node2D pivotPoint;
	[Export] public RayCast2D shotDirection;
	[Export] public Player player;
	[Export] public AudioStreamPlayer2D shootSoundPlayer;
	[Export] public int ammoCount;
	public int maxAmmoCount;
}
