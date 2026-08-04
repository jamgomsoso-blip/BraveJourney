using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Slide Obstacle")]
    [SerializeField] private bool safeWhileSliding;

    private bool hasDamaged;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (hasDamaged)
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        if (safeWhileSliding)
        {
            PlayerController playerController =
                other.GetComponent<PlayerController>();

            if (
                playerController != null &&
                playerController.IsSliding
            )
            {
                return;
            }
        }

        hasDamaged = true;
        playerHealth.TakeDamage(damage);
    }
}