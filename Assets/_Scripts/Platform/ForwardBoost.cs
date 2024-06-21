using UnityEngine;

public class ForwardBoost : MonoBehaviour
{
    [SerializeField] private float force;
    public bool entered = false;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        entered = true;

        Rigidbody playerRb = other.GetComponentInParent<Rigidbody>();
        Transform playerOrientation = playerRb.gameObject.GetComponent<PlayerMovement>().orientation;
        PlayerCamera playerCam = Camera.main.GetComponent<PlayerCamera>();
        if (playerRb == null) {
            Debug.LogError("player rigidbody not found");
            return;
        }
        
        playerRb.AddForce(playerOrientation.forward * force * 10f, ForceMode.Impulse);
        playerCam.fov(120f);
    }

    private void OnTriggerExit(Collider other) {
        entered = false;
    }
}
