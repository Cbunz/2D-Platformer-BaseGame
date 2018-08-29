using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damager : MonoBehaviour
{

	[Serializable]
    public class DamageableEvent : UnityEvent<Damager, Damageable>
    {
    }

    [Serializable]
    public class NonDamageableEvent : UnityEvent<Damager>
    {
    }

    protected Collider2D lastHit;
    public Collider2D LastHit { get { return lastHit; } }

    public DamageableEvent OnDamageableHit;
    public NonDamageableEvent OnNonDamageableHit;

    public int damage = 1;

    public SpriteRenderer spriteRenderer;
    public LayerMask hittableLayers;

    protected bool spriteOriginallyFlipped;
    protected bool canDamage = true;
    protected ContactFilter2D attackContactFilter;
    protected Collider2D[] attackOverlapResults = new Collider2D[10];
    protected Transform damagerTransform;

    public Vector2 offset = new Vector2(1.5f, 1f);
    public Vector2 size = new Vector2(2.5f, 1f);
    public bool offsetBasedOnSpriteFacing = true;
    
    public bool canHitTriggers;
    public bool disableDamageAfterHit = false;
    public bool forceRespawn = false;
    public bool ignoreInvincibility = false;

    void Awake()
    {
        attackContactFilter.layerMask = hittableLayers;
        attackContactFilter.useLayerMask = true;
        attackContactFilter.useTriggers = canHitTriggers;

        if (offsetBasedOnSpriteFacing && spriteRenderer != null)
        {
            spriteOriginallyFlipped = spriteRenderer.flipX;
        }

        damagerTransform = transform;
    }

    public void EnableDamage()
    {
        canDamage = true;
    }

    public void DisableDamage()
    {
        canDamage = false;
    }

    void FixedUpdate()
    {
        if (!canDamage)
        {
            return;
        }

        Vector2 scale = damagerTransform.lossyScale;

        Vector2 facingOffset = Vector2.Scale(offset, scale);

        if (offsetBasedOnSpriteFacing && spriteRenderer != null && spriteRenderer.flipX != spriteOriginallyFlipped)
        {
            facingOffset = new Vector2(-offset.x * scale.x, offset.y * scale.y);
        }

        Vector2 scaledSize = Vector2.Scale(size, scale);

        Vector2 pointA = (Vector2)damagerTransform.position + facingOffset - scaledSize * 0.5f;
        Vector2 pointB = pointA + scaledSize;

        int hitCount = Physics2D.OverlapArea(pointA, pointB, attackContactFilter, attackOverlapResults);

        for (int i = 0; i < hitCount; i++)
        {
            lastHit = attackOverlapResults[i];
            Damageable damageable = lastHit.GetComponent<Damageable>();

            if (damageable)
            {
                OnDamageableHit.Invoke(this, damageable);
                damageable.TakeDamage(this, ignoreInvincibility);
                if (disableDamageAfterHit)
                {
                    DisableDamage();
                }
            }
            else
            {
                OnNonDamageableHit.Invoke(this);
            }
        }
    }
}
