using Fusion;
using TMPro;
using UnityEngine;

/**
 * This component represents a shield protecting the player from balls.
 */
public class shield : NetworkBehaviour
{
    public string targetTag = "Player"; // The tag to check for
    public float shieldDuration = 5f;  // Duration of the shield

    private bool isShieldActive = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(targetTag) && !isShieldActive)
        {
            Health otherHealth = other.GetComponent<Health>();

            if (otherHealth != null)
            {
                Runner.Despawn(Object);
                otherHealth.ProtectPlayerRPC(true, shieldDuration); // Activate the shield for the specified duration
            }
            Health playerHealth = other.GetComponent<Health>();

    }
    }
}