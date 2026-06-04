using Godot;

public interface IEnemy
{
    void TakeDamage(int damage);
    public void Attack();
    public void OnAttackCooldownTimeout();
    public void OnAttackAreaBodyEntered(Node2D body);
    public void OnAttackAreaBodyExited(Node2D body);
}