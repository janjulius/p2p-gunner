using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : INetworkObject
{
    protected Vector3 targetPosition;
    protected float speed = 20;
    protected Vector3 startPosition;
    protected bool destroyOnArrival = true;
    private float timer = 0;

    private float destroyAfter = 10;

    private void Start()
    {
        startPosition = transform.position;
        transform.rotation = LookAt2D(targetPosition - transform.position);
    }

    public virtual void InitBullet(Vector3 targetPosition, float speed, Vector3 startPosition, bool destroyOnArrival, float destroyAfter)
    {
        this.targetPosition = targetPosition;
        this.speed = speed;
        this.startPosition = startPosition;
        this.destroyOnArrival = destroyOnArrival;
        this.destroyAfter = destroyAfter;
    }

    public virtual void Arrived()
    {
        if (destroyOnArrival)
            DestroyObject();
    }

    public override void ObjectUpdate()
    {
        UpdateTimer();

        // float nextZ = Mathf.MoveTowards(transform.position.z, targetPosition.z, speed * Time.deltaTime);
        // float baseY = Mathf.Lerp(startPosition.y, targetPosition.y, speed);
        // Vector3 nextPos = new Vector3(transform.position.x, baseY, nextZ);

        // transform.position = nextPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (transform.position == targetPosition) Arrived();
    }

    public void UpdateTimer()
    {
        timer += Time.deltaTime;

        if (timer >= destroyAfter)
            DestroyObject();
    }

    protected static Quaternion LookAt2D(Vector2 forward)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }
}