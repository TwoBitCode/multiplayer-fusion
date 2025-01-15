using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColor : NetworkBehaviour
{
    [Networked]
    public Color NetworkedColor { get; set; }

    private ChangeDetector _changes;
    private MeshRenderer meshRendererToChange;

    [SerializeField] string colorChangeLog = "Color Change";
    [SerializeField] float minRandomRange = 0f;
    [SerializeField] float maxRandomRange = 1f;

    [SerializeField] float colorA = 1f;

    public override void Spawned() {
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        meshRendererToChange = GetComponentInChildren<MeshRenderer>();
        if (NetworkedColor != null)
            meshRendererToChange.material.color = NetworkedColor;
    }

    public override void Render() {
        foreach (var change in _changes.DetectChanges(this, out var previousBuffer, out var currentBuffer)) {
            switch (change) {
                case nameof(NetworkedColor):
                    meshRendererToChange.material.color = NetworkedColor;
                    break;
            }
        }
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData inputData)) {
            if (HasStateAuthority && inputData.colorActionValue) {
                    Debug.Log(colorChangeLog);
                    var randomColor = new Color(Random.Range(minRandomRange, maxRandomRange), Random.Range(minRandomRange, maxRandomRange), Random.Range(minRandomRange, maxRandomRange), colorA);
                    // Changing the material color here directly does not work since this code is only executed on the client pressing the button and not on every client.
                    // meshRendererToChange.material.color = randomColor;
                    NetworkedColor = randomColor;
            }
        }
    }
}
