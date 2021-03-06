using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public abstract class INetworkObject : MonoBehaviour
{
    public enum rpcTarget
    {
        one = 0,
        other = 1,
        all = 2
    }

    public int playerId;
    public int netWorkId;
    public int objectId;
    public bool owner;
    protected NetworkManager nwm;

    protected bool interpolatePosition = true;
    protected bool interpolateRotation = true;
    protected float interPolationAmountPosition = 60f;
    protected float interPolationAmountRotation = 60f;

    public void Init(int playerId, int objectId, int netWorkId)
    {
        this.playerId = playerId;
        this.netWorkId = netWorkId;
        this.objectId = objectId;
        owner = netWorkId == NetworkManager.Instance.MyNetworkId;
        nwm = NetworkManager.Instance;
    }

    private void Update()
    {
        if (!nwm.NetworkObjects.ContainsKey(objectId))
            return;

        if (!owner)
        {
            transform.position = interpolatePosition
                ? Vector3.Lerp(
                    transform.position,
                    nwm.NetworkObjects[objectId].GetPosition(),
                    0.2f)
                : nwm.NetworkObjects[objectId].GetPosition();
            transform.rotation = interpolateRotation
                ? Quaternion.Lerp(
                    transform.rotation,
                    nwm.NetworkObjects[objectId].GetRotation(),
                    0.2f)
                : nwm.NetworkObjects[objectId].GetRotation();
            return;
        }

        ObjectUpdate();

        nwm.NetworkObjects[objectId].SetPosition(transform.position);
        nwm.NetworkObjects[objectId].SetRotation(transform.rotation);
    }

    public void DestroyObject()
    {
        nwm.DestroyNetworkObject(objectId);
        gameObject.SetActive(false);
    }

    public void SendRPC(string rpcName, int sender, rpcTarget target, params object[] args)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort) 201);
        writer.Put((byte) target);
        writer.Put(rpcName);
        writer.Put(objectId);
        writer.Put(sender);
        foreach (var o in args)
        {
            switch (o)
            {
                case int i:
                    writer.Put(i);
                    break;
                case string s:
                    writer.Put(s);
                    break;
                case float f:
                    writer.Put(f);
                    break;
                case bool b:
                    writer.Put(b);
                    break;
            }
        }

        nwm.Send(writer, DeliveryMethod.ReliableUnordered);
    }

    public abstract void ObjectUpdate();
}