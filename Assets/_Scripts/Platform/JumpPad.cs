using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpForce;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        Rigidbody playerRb = other.GetComponentInParent<Rigidbody>();
        if (playerRb == null) {
            Debug.Log("cannot find player rb");
            return;
        }
        playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}
