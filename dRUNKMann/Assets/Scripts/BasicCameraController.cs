using UnityEngine;

public class BasicCameraController : MonoBehaviour
{
    private Vector3 _offset;

    [SerializeField] private GameObject playerCapsule;

    void Start()
    {
        if (playerCapsule == null)
        {
            enabled = false;
            return;
        }

        _offset = transform.position - playerCapsule.transform.position;
    }

    void Update()
    {
        if (playerCapsule == null)
        {
            return;
        }

        transform.position = playerCapsule.transform.position + _offset;
    }
}
