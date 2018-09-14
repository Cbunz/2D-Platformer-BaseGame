using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour {

    public LayerMask groundedLayerMask;
    public float groundedRaycastDistance = 0.1f;

    BoxCollider2D _collider;
    // BoxCollider2D colliderBox;
    // CapsuleCollider2D colliderCapsule;
    // Collider2D _collider;
    // private bool hasBoxCollider;
    Rigidbody2D _rigidbody;
    ContactFilter2D contactFilter;
    RaycastHit2D[] hitResults = new RaycastHit2D[5];
    RaycastHit2D[] foundHits = new RaycastHit2D[3];
    Collider2D[] groundColliders = new Collider2D[3];
    Vector2[] raycastPositions = new Vector2[3];
    Vector2 previousPosition;
    Vector2 currentPosition;
    Vector2 nextMovement;

    public bool Grounded { get; protected set; }
    public bool OnCeiling { get; protected set; }
    public Vector2 Velocity { get; protected set; }
    public Rigidbody2D Rigidbody { get { return _rigidbody; } }
    public Collider2D[] GroundColliders { get { return groundColliders; } }
    public ContactFilter2D ContactFilter { get { return contactFilter; } }


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();

        /* colliderBox = GetComponent<BoxCollider2D>();
        colliderCapsule = GetComponent<CapsuleCollider2D>();
        if (colliderBox != null)
        {
            hasBoxCollider = true;
        }
        else if (colliderCapsule != null)
        {
            hasBoxCollider = false;
        }
        */

        currentPosition = _rigidbody.position;
        previousPosition = _rigidbody.position;

        contactFilter.layerMask = groundedLayerMask;
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;

        Physics2D.queriesStartInColliders = false;
    }

    void FixedUpdate()
    {
        previousPosition = _rigidbody.position;
        currentPosition = previousPosition + nextMovement;
        Velocity = (currentPosition - previousPosition) / Time.deltaTime;

        _rigidbody.MovePosition(currentPosition);
        nextMovement = Vector2.zero;

        CheckCapsuleEndCollisions();
        CheckCapsuleEndCollisions(false);
    }

    public void Move(Vector2 movement)
    {
        nextMovement += movement;
    }

    public void Teleport(Vector2 position)
    {
        Vector2 delta = position - currentPosition;
        previousPosition += delta;
        currentPosition = position;
        _rigidbody.MovePosition(position);
    }

    

    public void CheckCapsuleEndCollisions(bool bottom = true)
    {
        Vector2 raycastDirection;
        Vector2 raycastStart;
        float raycastDistance;

        if (_collider == null)
        {
            raycastStart = _rigidbody.position + Vector2.up;
            raycastDistance = 1f + groundedRaycastDistance;

            if (bottom)
            {
                raycastDirection = Vector2.down;

                raycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                raycastPositions[1] = raycastStart;
                raycastPositions[2] = raycastStart + Vector2.right * 0.4f;
            }
            else
            {
                raycastDirection = Vector2.up;

                raycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                raycastPositions[1] = raycastStart;
                raycastPositions[2] = raycastStart + Vector2.right * 0.4f;
            }
        }
        else
        {
            raycastStart = _rigidbody.position + _collider.offset;
            raycastDistance = _collider.size.x * .5f + groundedRaycastDistance * 2f;

            if (bottom)
            {
                raycastDirection = Vector2.down;
                Vector2 raycastStartBottomCenter = raycastStart + Vector2.down * (_collider.size.y * 0.5f - _collider.size.x * 0.5f);

                raycastPositions[0] = raycastStartBottomCenter + Vector2.left * _collider.size.x * 0.5f;
                raycastPositions[1] = raycastStartBottomCenter;
                raycastPositions[2] = raycastStartBottomCenter + Vector2.right * _collider.size.x * 0.5f;
            }
            else
            {
                raycastDirection = Vector2.up;
                Vector2 raycastStartTopCenter = raycastStart + Vector2.up * (_collider.size.y * 0.5f - _collider.size.x * 0.5f);

                raycastPositions[0] = raycastStartTopCenter + Vector2.left * _collider.size.x * 0.5f;
                raycastPositions[1] = raycastStartTopCenter;
                raycastPositions[2] = raycastStartTopCenter + Vector2.right * _collider.size.x * 0.5f;
            }
        }

        for (int i = 0; i < raycastPositions.Length; i++)
        {
            int count = Physics2D.Raycast(raycastPositions[i], raycastDirection, contactFilter, hitResults, raycastDistance);

            if (bottom)
            {
                foundHits[i] = count > 0 ? hitResults[0] : new RaycastHit2D();
                groundColliders[i] = foundHits[i].collider;
            }
            else
            {
                OnCeiling = false;

                for (int j = 0; j < hitResults.Length; j++)
                {
                    if (hitResults[j].collider != null)
                    {
                        if (!PhysicsHelper.ColliderHasPlatformEffector(hitResults[j].collider))
                        {
                            OnCeiling = true;
                        }
                    }
                }
            }
        }

        if (bottom)
        {
            Vector2 groundNormal = Vector2.zero;
            int hitCount = 0;

            for (int i = 0; i < foundHits.Length; i++)
            {
                if (foundHits[i].collider != null)
                {
                    groundNormal += foundHits[i].normal;
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                groundNormal.Normalize();
            }

            Vector2 relativeVelocity = _rigidbody.velocity;
            
            for (int i = 0; i < groundColliders.Length; i++)
            {
                if (groundColliders[i] == null)
                {
                    continue;
                }

                MovingPlatform movingPlatform;

                if (PhysicsHelper.TryGetMovingPlatform(groundColliders[i], out movingPlatform))
                {
                    relativeVelocity -= movingPlatform.Velocity / Time.deltaTime;
                    break;
                }
            }

            if (Mathf.Approximately(groundNormal.x, 0f) && Mathf.Approximately(groundNormal.y, 0f))
            {
                Grounded = false;
            }
            else
            {
                Grounded = (relativeVelocity.y <= 0f);

                if (_collider != null)
                {
                    if (groundColliders[1] != null)
                    {
                        float colliderBottomHeight = _rigidbody.position.y + _collider.offset.y - _collider.size.y * 0.5f;
                        float middleHitHeight = foundHits[1].point.y;
                        Grounded &= (middleHitHeight < colliderBottomHeight + groundedRaycastDistance);
                    }
                }
            }
        }

        for (int i = 0; i < hitResults.Length; i++)
        {
            hitResults[i] = new RaycastHit2D();
        }
    }
}
