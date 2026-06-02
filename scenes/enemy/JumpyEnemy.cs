using Godot;
using System;

public partial class JumpyEnemy : RigidBody2D, IEnemy
{
	// Called when the node enters the scene tree for the first time.
	[Export] public AudioStreamPlayer2D asp2d;
	[Export] public EnemyHealthBar enemyHealthBar;
	[Export] public CollisionShape2D collisionShape2D;
	public int maxHealth;
	public override void _Ready()
    {
        maxHealth = GetHealth();
    }

	public void TakeDamage(int damage)
    {
        SetMeta("Health", GetHealth() - damage);
        if (GetHealth() <= 0)
        {
            asp2d.Play();
            this.Visible = false;
            // collisionShape2D.Disabled = true;
            collisionShape2D.SetDeferred("disabled", true);
        }
        enemyHealthBar.SetProgress(Mathf.InverseLerp(0, maxHealth, GetHealth()));
    }

	private int GetHealth()
    {
        return (int) GetMeta("Health");
    }
	public void OnAudioFinished()
	{
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
