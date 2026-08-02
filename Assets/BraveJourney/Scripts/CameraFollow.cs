using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float horizontalOffset = 5f;

    private float fixedY;
    private float fixedZ;

    private void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (player == null)
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