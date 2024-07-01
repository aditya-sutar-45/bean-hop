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
        
        playerRb.AddForce(pm.moveDirection * force * 10f, ForceMode.Impulse);
        playerCam.fov(120f);
    }

    private void OnTriggerExit(Collider other) {
        entered = false;
    }
}
