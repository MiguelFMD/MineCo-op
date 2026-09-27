using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterComponent : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }
    void Update()
    {
        if(Keyboard.current.tKey.wasPressedThisFrame)
        {
            ShootServerRpc(this.transform.position, Quaternion.identity);
        }
    }

    [Rpc(SendTo.Server)]
    private void ShootServerRpc(Vector3 pos, Quaternion rot)
    {
        EventManager.OnShootPressed?.Invoke(pos, rot);
    }
}
