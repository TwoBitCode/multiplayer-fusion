using Fusion;
using UnityEngine;

/**
 * This component represents a ball moving at a constant speed.
 */
public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer lifeTimer { get; set; }

    [SerializeField] float lifeTime = 5.0f;
    [SerializeField] float speed = 5.0f;
    [SerializeField] int damagePerHit = 1;
    [SerializeField] int pointsToAdd = 1;
    // Reference to the shooter
    public Health Shooter { get; set; } // Set this when the ball is spawned

    // Store the ball's initial direction and rotation
    private Vector3 initialDirection;
    private Quaternion initialRotation;

    [SerializeField] string targetShieldedLog = "Target is shielded, no damage or points change.";
    [SerializeField] string targetNotShieldedLog = "not shielded";

    public override void Spawned()
    {
        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);

        // Set the initial direction based on the player's facing direction
        // Or any custom direction you want the ball to move in (e.g., transform.forward, etc.)
        initialDirection = transform.forward;
        initialRotation = transform.rotation; // Save the initial rotation

    }

    public override void FixedUpdateNetwork()
    {
        if (lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
        else
        {
            transform.position += initialDirection * speed * Runner.DeltaTime;

            // Optionally, apply the initial rotation (if needed for visual stability)
            transform.rotation = initialRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return; // Only execute on the StateAuthority

        Health otherHealth = other.GetComponent<Health>();

        if (otherHealth != null)
        {
            // Check if the player being hit is shielded
            if (!otherHealth.IsShielded)
            {
                Debug.Log(targetNotShieldedLog);
                otherHealth.DealDamageRpc(damagePerHit);


                // Award points to the shooter
                if (Shooter != null)
                {
                    Shooter.AddPointsRPC(pointsToAdd);
                }
            }
            else
            {
                Debug.Log(targetShieldedLog);
            }
        }
    }

}