// From the Fusion 2 Tutorial: https://doc.photonengine.com/fusion/current/tutorials/host-mode-basics/2-setting-up-a-scene#launching-fusion
using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// This class launches Fusion NetworkRunner, and also spanws a new avatar whenever a player joins.
public class SpawningLauncher : EmptyLauncher
{
    [SerializeField] NetworkPrefabRef _playerPrefab;
    [SerializeField] Transform[] spawnPoints;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    const int noBindingsNum = 0;

    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined");
        bool isAllowedToSpawn = (runner.GameMode == GameMode.Shared)? 
            (player == runner.LocalPlayer):   // in Shared mode, the local player is allowed to spawn.
            runner.IsServer;                  // in Host or Server mode, only the server is allowed to spawn.
        if (isAllowedToSpawn)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = spawnPoints[player.AsIndex % spawnPoints.Length].position;
            //new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 0, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, /*input authority:*/ player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }
    
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left");
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    [SerializeField] InputAction moveAction = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction shootAction = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction colorAction = new InputAction(type: InputActionType.Button);

    [SerializeField] InputAction jumpAction = new InputAction(type: InputActionType.Button);

    private void OnEnable() { moveAction.Enable(); shootAction.Enable(); colorAction.Enable(); jumpAction.Enable(); }
    private void OnDisable() { moveAction.Disable(); shootAction.Disable(); colorAction.Disable(); jumpAction.Disable(); }
    void OnValidate() {
        // Provide default bindings for the input actions. Based on answer by DMGregory: https://gamedev.stackexchange.com/a/205345/18261
        if (moveAction == null)
            moveAction = new InputAction(type: InputActionType.Button);
        if (moveAction.bindings.Count == noBindingsNum)
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
        if (shootAction.bindings.Count == noBindingsNum)
            shootAction.AddBinding("<Keyboard>/space");
        if (colorAction.bindings.Count == noBindingsNum)
            colorAction.AddBinding("<Keyboard>/C");

        if (jumpAction == null)
            jumpAction = new InputAction(type: InputActionType.Button);
        if (jumpAction.bindings.Count == noBindingsNum)
            jumpAction.AddBinding("<Keyboard>/J");
    }

    NetworkInputData inputData = new NetworkInputData();

    private void Update()
    {
        if (shootAction.WasPressedThisFrame())
        {
            inputData.shootActionValue = true;
        }
        if (colorAction.WasPressedThisFrame())
        {
            inputData.colorActionValue = true;
        }
        if (jumpAction.WasPressedThisFrame())
        {
            inputData.jumpActionValue = true;
        }

    }

    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        inputData.moveActionValue = moveAction.ReadValue<Vector2>();
        input.Set(inputData);    // pass inputData by value 

        inputData.shootActionValue = false; // clear shoot flag
        inputData.colorActionValue = false; // clear shoot flag
    }
}
