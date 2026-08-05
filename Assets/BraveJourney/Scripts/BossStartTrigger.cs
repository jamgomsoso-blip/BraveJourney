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

        PlayerController playerController =
            other.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError(
                "PlayerController를 찾을 수 없습니다."
            );
            return;
        }

        hasStarted = true;

        if (bossObject != null)
        {
            bossObject.SetActive(true);
        }

        playerController.StartBossBattle();

        Debug.Log("보스전 구간 진입");

        gameObject.SetActive(false);
    }
}