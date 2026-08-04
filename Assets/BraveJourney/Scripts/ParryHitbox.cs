using UnityEngine;

public class ParryHitbox : MonoBehaviour
{
    private PlayerController playerController;
    private Collider2D hitboxCollider;

    public bool CanReflect =>
        playerController != null &&
        playerController.IsParrying;

    private void Awake()
    {
        playerController =
            GetComponentInParent<PlayerController>();

        hitboxCollider =
            GetComponent<Collider2D>();

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (hitboxCollider == null)
        {
            return;
        }

        hitboxCollider.enabled = CanReflect;
    }

    private void OnDisable()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }
}