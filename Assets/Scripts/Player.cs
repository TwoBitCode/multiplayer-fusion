using UnityEngine;
using Fusion;
using TMPro;

public class Player : NetworkBehaviour
{
    private CharacterController _cc;

    [SerializeField] float speed = 5f;
    [SerializeField] GameObject ballPrefab;

    //Camera firstPersonCamera;
    [SerializeField] Camera Camera;

    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI shieldText;

    private Vector3 moveDirection;
    private Quaternion cameraRotationY;

    [SerializeField] int noMovement = 0;
    [SerializeField] int YMoveDirection = 0;
    [SerializeField] int ZRotation = 0;

    [SerializeField] string shootLog = "SHOOT!";

    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();
        if (HasStateAuthority)
        {
            Camera = Camera.main;
            Camera.GetComponent<FirstPersonCamera>().target = transform;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }


        if (GetInput(out NetworkInputData inputData))
        {
            if (inputData.moveActionValue.magnitude > noMovement)
            {
                inputData.moveActionValue.Normalize();   //  Ensure that the vector magnitude is 1, to prevent cheating.
                moveDirection = new Vector3(inputData.moveActionValue.x, YMoveDirection, inputData.moveActionValue.y);
                cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, ZRotation);

                Vector3 DeltaX = cameraRotationY * moveDirection * speed * Runner.DeltaTime;

                _cc.Move(DeltaX);

                // look in the direction you're moving
                if (DeltaX != Vector3.zero)
                {
                    gameObject.transform.forward = DeltaX;
                }

            }

            if (HasStateAuthority)
            { // Only the server can spawn new objects ; otherwise you will get an exception "ClientCantSpawn".
                if (inputData.shootActionValue)
                {
                    Debug.Log(shootLog);
                    NetworkObject ball = Runner.Spawn(ballPrefab, transform.position + (cameraRotationY * moveDirection), Quaternion.LookRotation(cameraRotationY * moveDirection), Object.InputAuthority);
                    //ball.transform.SetParent(transform);

                    // Assign the shooter to the ball
                    if (ball.TryGetComponent(out Ball ballComponent))
                    {
                        ballComponent.Shooter = GetComponent<Health>();
                    }
                }
            }
        }
    }
}