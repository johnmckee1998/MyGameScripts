using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class GenericNetworkObjectPosRot : NetworkBehaviour
{
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> rotation = new NetworkVariable<Quaternion>();

    [SerializeField] private bool autoUpdate;

    void OnEnable()
    {
        position.OnValueChanged += OnPosChange;
        rotation.OnValueChanged += OnRotChange;

        UpdatePositionAndRotation(transform.position, transform.rotation);
    }

    void OnDisable()
    {
        position.OnValueChanged -= OnPosChange;
        rotation.OnValueChanged -= OnRotChange;
    }

    private void Update()
    {
        if (autoUpdate && (IsServer || IsOwner))
            UpdatePositionAndRotation(transform.position, transform.rotation);
    }

    public void UpdatePositionAndRotation(Vector3 pos, Quaternion rot)
    {
        //position.Value = pos;
        //rotation.Value = rot;
        UpdatePosRotServerRpc(pos, rot);
    }

    [ServerRpc]
    public void UpdatePosRotServerRpc(Vector3 pos, Quaternion rot)
    {
        position.Value = pos;
        rotation.Value = rot;
    }

    public void MovePosNonOwner(float f)
    {
        Vector3 pos = position.Value + (Vector3.up * f);
        NonOwnerMovePosServerRpc(pos);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NonOwnerMovePosServerRpc(Vector3 pos)
    {
        position.Value = pos;
    }

    private void OnRotChange(Quaternion previousValue, Quaternion newValue)
    {
        if (!IsClient) return;

        transform.rotation = rotation.Value;
    }

    private void OnPosChange(Vector3 previousValue, Vector3 newValue)
    {
        if (!IsClient) return;

        transform.position = position.Value;
    }

    public void RequestOwnershipChange(ulong clientID)
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeOwnershipServerRpc(ulong clientID)
    {
        
    }
}
