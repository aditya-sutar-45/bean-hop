using UnityEngine;

public class ForwardBoost : MonoBehaviour
{
    [SerializeField] private float force;
    public bool entered = false;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        entered = true;

        Rigidbody playerRb = other.GetComponentInParent<Rigidbody>();
        PlayerMovement pm = playerRb.gameObject.GetComponent<PlayerMovement>();
        PlayerCamera playerCam = Camera.main.GetComponent<PlayerCamera>();

        if (playerRb == null) {
            Debug.LogError("player rigidbody not found");
            return;
        }

        Vector3 dir = pm.moveDirection != Vector3.zero ? pm.moveDirection : pm.orientation.forward;
        
        playerRb.AddForce(((dir * force * 10f) + (Vector3.up * 10f)), ForceMode.Impulse);
        playerCam.fov(120f);
    }

    private void OnTriggerExit(Collider other) {
        entered = false;
    }
}
