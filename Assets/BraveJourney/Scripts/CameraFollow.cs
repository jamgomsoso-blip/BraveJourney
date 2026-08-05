using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float horizontalOffset = 5f;

    private float fixedY;
    private float fixedZ;
    private PlayerController playerController;

    private void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;

        if (player != null)
        {
            playerController =
                player.GetComponent<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // 보스전이 시작되면 현재 위치에서 카메라 정지
        if (
            playerController != null &&
            playerController.IsBossBattle
        )
        {
            return;
        }

        transform.position = new Vector3(
            player.position.x + horizontalOffset,
            fixedY,
            fixedZ
        );
    }
}