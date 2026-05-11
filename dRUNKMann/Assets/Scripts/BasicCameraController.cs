using UnityEngine;

public class BasicCameraController : MonoBehaviour
{
    private Vector3 _offset;

    [SerializeField] private GameObject playerCapsule;

    void Start()
    {
        _offset = transform.position - playerCapsule.transform.position;
    }

    void Update()
    {
        transform.position = playerCapsule.transform.position + _offset;
    }
}
