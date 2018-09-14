using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour {
    
	public enum MovingPlatformType
    {
        BackForth,
        Loop,
        Once
    }

    public PlatformCatcher platformCatcher;
    public float speed = 1.0f;
    public MovingPlatformType platformType;

    public bool startMovingOnlyWhenVisible;
    public bool isMovingAtStart = true;

    [HideInInspector]
    public Vector3[] localNodes = new Vector3[1];

    public float[] waitTimes = new float[1];

    protected Vector3[] worldNode;
    public Vector3[] WorldNode { get { return worldNode; } }

    protected int current = 0;
    protected int next = 0;
    protected int dir = 1;

    protected float waitTime = -1.0f;

    protected Rigidbody2D _rigidbody2D;
    protected Vector2 velocity;

    protected bool started = false;
    protected bool veryFirstStart = false;

    public Vector2 Velocity
    {
        get { return velocity; }
    }

    private void Reset()
    {
        localNodes[0] = Vector3.zero;
        waitTimes[0] = 0;

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.isKinematic = true;

        if (platformCatcher == null)
        {
            platformCatcher = GetComponent<PlatformCatcher>();
        }
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.isKinematic = true;

        if (platformCatcher == null)
        {
            platformCatcher = GetComponent<PlatformCatcher>();
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            var b = renderers[i].gameObject.AddComponent<VisibleBubbleUp>();
            b.objectBecameVisible = BecameVisible;
        }

        worldNode = new Vector3[localNodes.Length];
        for (int i = 0; i < worldNode.Length; ++i)
        {
            worldNode[i] = transform.TransformPoint(localNodes[i]);
        }

        Init();
    }

    protected void Init()
    {
        current = 0;
        dir = 1;
        next = localNodes.Length > 1 ? 1 : 0;

        waitTime = waitTimes[0];

        veryFirstStart = false;
        if (isMovingAtStart)
        {
            started = !startMovingOnlyWhenVisible;
            veryFirstStart = true;
        }
        else
        {
            started = false;
        }
    }

    private void FixedUpdate()
    {
        if (!started)
        {
            return;
        }

        if (current == next)
        {
            return;
        }

        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
            return;
        }

        float distanceToGo = speed * Time.deltaTime;

        while (distanceToGo > 0)
        {
            Vector2 direction = worldNode[next] - transform.position;

            float dist = distanceToGo;
            if (direction.sqrMagnitude < dist * dist)
            {
                dist = direction.magnitude;

                current = next;

                waitTime = waitTimes[current];

                if (dir > 0)
                {
                    next += 1;
                    if (next >= worldNode.Length)
                    {
                        switch (platformType)
                        {
                            case MovingPlatformType.BackForth:
                                next = worldNode.Length - 2;
                                dir = -1;
                                break;
                            case MovingPlatformType.Loop:
                                next = 0;
                                break;
                            case MovingPlatformType.Once:
                                next -= 1;
                                StopMoving();
                                break;
                        }
                    }
                }
            }

            velocity = direction.normalized * dist;

            _rigidbody2D.MovePosition(_rigidbody2D.position + velocity);
            platformCatcher.MoveCaughtObjects(velocity);

            distanceToGo -= dist;

            if (waitTime > 0.001f)
            {
                break;
            }
        }
    }

    public void StartMoving()
    {
        started = true;
    }

    public void StopMoving()
    {
        started = false;
    }

    public void ResetPlatform()
    {
        transform.position = worldNode[0];
        Init();
    }

    private void BecameVisible(VisibleBubbleUp obj)
    {
        if (veryFirstStart)
        {
            started = true;
            veryFirstStart = false;
        }
    }
}
