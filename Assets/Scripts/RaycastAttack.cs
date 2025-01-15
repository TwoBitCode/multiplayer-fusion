using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

// from Fusion tutorial: https://doc.photonengine.com/fusion/current/tutorials/shared-mode-basics/5-remote-procedure-calls
public class RaycastAttack : NetworkBehaviour
{
    [SerializeField] int Damage;

    [SerializeField] InputAction attack;
    [SerializeField] InputAction attackLocation;

    [SerializeField] float shootDistance = 5f;

    const int noBindingsNum = 0;
    [SerializeField] Color redColor = Color.red;
    [SerializeField] float rayDuration = 1f;

    private void OnEnable() { attack.Enable(); attackLocation.Enable();  }
    private void OnDisable() { attack.Disable(); attackLocation.Disable(); }
    void OnValidate()
    {
        // Provide default bindings for the input actions. Based on answer by DMGregory: https://gamedev.stackexchange.com/a/205345/18261
        if (attack == null)
            attack = new InputAction(type: InputActionType.Button);
        if (attack.bindings.Count == noBindingsNum)
            attack.AddBinding("<Mouse>/leftButton");

        if (attackLocation == null)
            attackLocation = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");
        if (attackLocation.bindings.Count == noBindingsNum)
            attackLocation.AddBinding("<Mouse>/position");
    }


    private bool _attackPressed;
    void Update()
    {  // We have to read the button status in Update, because FixedNetworkUpdate might miss it.
        if (!HasStateAuthority) return;
        if (attack.WasPerformedThisFrame())
        {
            _attackPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)  return;

        if (_attackPressed)
        {
            Vector2 attackLocationInScreenCoordinates = attackLocation.ReadValue<Vector2>();

            var camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(attackLocationInScreenCoordinates);
            ray.origin += camera.transform.forward;

            Debug.DrawRay(ray.origin, ray.direction * shootDistance, redColor, duration: rayDuration);

            if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction * shootDistance, out var hit))
            {
                GameObject hitObject = hit.transform.gameObject;

                if (hitObject.TryGetComponent<Health>(out var health))
                {
                    health.DealDamageRpc(Damage);
                }
            }
            _attackPressed = false;
        }
    }


}