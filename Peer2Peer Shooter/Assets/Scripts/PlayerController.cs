using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : INetworkObject
{
    public GameObject bullet;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NetworkManager.Instance.InstantiateNetworkObject("Bullet", playerId, transform.position, Quaternion.identity);
        }
    }

    public override void ObjectUpdate()
    {
        throw new System.NotImplementedException();
    }
}
