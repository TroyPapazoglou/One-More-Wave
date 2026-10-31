using UnityEngine;

public class Chaser : Enemy
{  
    
    public override bool Move(Player player)
    {
        if (!base.Move(player))
            return false;

        Vector3 direction = player.transform.position - gameObject.transform.position;

        rb.linearVelocity = direction.normalized * moveSpeed;

        return true;
    }

    public override void Reset()
    {
        base.Reset();
        rb.simulated = true;       
    }

    public override void Disable()
    {
        base.Disable();
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
    }

    public override void PlayDeathAnimation(Vector2 contactPoint, Player player, Enemy enemy)
    {
        base.PlayDeathAnimation(contactPoint, player, enemy);

        DeathManager.Instance.PlayDeathAnimation(EnemyType.Chaser, contactPoint, player, enemy);
    }
   
}
