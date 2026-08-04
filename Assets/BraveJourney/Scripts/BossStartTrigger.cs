using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;

    private bool hasStarted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasStarted)
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        hasStarted = true;

        if (bossObject != null)
        {
            bossObject.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}