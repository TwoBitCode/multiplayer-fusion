using Fusion;
using TMPro;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Health : NetworkBehaviour
{
    [SerializeField] NumberField HealthDisplay;

    [SerializeField] TextMeshPro shieldCountdownText; // Text for the shield countdown

    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI shieldText;
    [SerializeField] string healthStr = "Health: ";
    [SerializeField] string shieldStr = "Shield: ";
    const float initialShieldCount = 0;

    [Networked]
    [SerializeField] int NetworkedHealth { get; set; } = 100;

    [SerializeField] string playerShieldLog = "Player is shielded";
    [SerializeField] string addingPointLog = "Adding points, secceeded in dealing damage";
    [SerializeField] string receivedDamageLog = "Received DealDamageRpc on StateAuthority, modifying NetworkedHealth";


    // Migration from Fusion 1:  https://doc.photonengine.com/fusion/current/getting-started/migration/coming-from-fusion-v1
    private ChangeDetector _changes;

    [Networked]
    [SerializeField] public bool IsShielded { get; set; } = false; // Track if the player is shielded

    [Networked]
    private TickTimer ShieldTimer { get; set; } // Timer for the shield

    [Networked]
    private float NetworkedShieldTimeRemaining { get; set; } = 0f; // Sync shield remaining time across network

    [SerializeField] Color blueColor = Color.blue;

    private void Start()
    {
        setText(healthText, healthStr, NetworkedHealth);
        setText(shieldText, shieldStr, initialShieldCount);
    }

    public override void Spawned()
    {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        HealthDisplay.SetNumber(NetworkedHealth);

        // Hide the shield countdown text initially
        if (shieldCountdownText != null)
        {
            shieldCountdownText.gameObject.SetActive(false);
        }

        // Check if the current player owns this object
        if (!Object.HasInputAuthority)
        {
            // Disable the UI Canvas for other players
            Canvas playerCanvas = GetComponentInChildren<Canvas>();
            if (playerCanvas != null)
            {
                playerCanvas.enabled = false;
            }
        }
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(NetworkedHealth):
                    HealthDisplay.SetNumber(NetworkedHealth);
                    break;


                case nameof(IsShielded):
                    // If the shield state changes, we want to update the countdown text visibility
                    if (IsShielded)
                    {
                        if (shieldCountdownText != null)
                        {
                            shieldCountdownText.gameObject.SetActive(true);  // Ensure the text is shown when the shield is active
                        }
                    }
                    else
                    {
                        if (shieldCountdownText != null)
                        {
                            shieldCountdownText.gameObject.SetActive(false);  // Hide the text when the shield is inactive
                        }
                    }
                    break;

                case nameof(NetworkedShieldTimeRemaining):
                    // Update shield countdown text with remaining time
                    if (IsShielded)
                    {
                        UpdateShieldCountdownText(NetworkedShieldTimeRemaining);
                    }
                    break;
            }
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    // All players can call this function; only the StateAuthority receives the call.
    public void DealDamageRpc(int damage)
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        if (!IsShielded)
        {
            Debug.Log(receivedDamageLog);
            NetworkedHealth -= damage;

            setText(healthText, healthStr, NetworkedHealth);
        }
        else
        {
            Debug.Log(playerShieldLog);
        }
    }

    public void AddPointsRPC(int pointsToAdd)
    {
        if (!IsShielded)
        {
            Debug.Log(addingPointLog);
            NetworkedHealth += pointsToAdd;

            setText(healthText, healthStr, NetworkedHealth);
        }
        else
        {
            Debug.Log(playerShieldLog);
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ProtectPlayerRPC(bool protect, float shieldDuration = 5f)
    {
        IsShielded = protect;

        if (protect)
        {

            ShieldTimer = TickTimer.CreateFromSeconds(Runner, shieldDuration);
            NetworkedShieldTimeRemaining = shieldDuration;  // Set networked shield time

        }
        else
        {
            NetworkedShieldTimeRemaining = initialShieldCount;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // If the shield is active and the timer hasn't expired
        if (IsShielded)
        {
            if (ShieldTimer.Expired(Runner))
            {
                ProtectPlayerRPC(false); // Deactivate the shield when the timer expires
                setText(shieldText, shieldStr, initialShieldCount);
            }
            else
            {
                NetworkedShieldTimeRemaining = (float)ShieldTimer.RemainingTime(Runner);
                setText(shieldText, shieldStr, NetworkedShieldTimeRemaining);
            }
        }
    }

    private void UpdateShieldCountdownText(float timeRemaining)
    {
        if (shieldCountdownText != null)
        {
            shieldCountdownText.text = $"{Mathf.Ceil(timeRemaining)}";
            shieldCountdownText.color = blueColor; // Set text color to blue
        }
    }

    private void setText(TextMeshProUGUI textUI, string text, float points)
    {
        textUI.text = "";
        textUI.text = $"{text}{points:F2}"; // Ensures proper formatting (e.g., 2 decimals)
    }
}
