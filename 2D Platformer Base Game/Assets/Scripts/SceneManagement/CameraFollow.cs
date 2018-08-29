using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    private GameObject player;

    public float verticalOffset;
    public float lookAheadDstX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;
    public Vector2 focusAreaSize;

    private FocusArea focusArea;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    // private CapsuleCollider2D coll;
    private float currentLookAheadX;
    private float playerLookAheadX;
    private float lookAheadDirX;
    private float smoothLookVelocityX;
    private float smoothVelocityY;

    private bool lookAheadStopped;

    void Start()
    {
        player = GameObject.Find("Girl");
        rb = player.GetComponent<Rigidbody2D>();
        coll = player.GetComponent<BoxCollider2D>();
        // coll = player.GetComponent<CapsuleCollider2D>();
        focusArea = new FocusArea(coll.bounds, focusAreaSize);
    }

    void LateUpdate()
    {
        focusArea.Update(coll.bounds);

        Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(rb.velocity.x) == Mathf.Sign(focusArea.velocity.x) && rb.velocity.x != 0)
            {
                lookAheadStopped = false;
                playerLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    playerLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                }
            }
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, playerLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
        focusPosition += Vector2.right * currentLookAheadX;
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }
    */

    private struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        private float left, right;
        private float top, bottom;

        public FocusArea(Bounds playerBounds, Vector2 size)
        {
            left = playerBounds.center.x - size.x / 2;
            right = playerBounds.center.x + size.x / 2;
            bottom = playerBounds.min.y;
            top = playerBounds.min.y + size.y;

            velocity = Vector2.zero;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void Update(Bounds playerBounds)
        {
            float shiftX = 0f;
            if (playerBounds.min.x < left)
            {
                shiftX = playerBounds.min.x - left;
            }
            else if (playerBounds.max.x > right)
            {
                shiftX = playerBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0f;
            if (playerBounds.min.y < bottom)
            {
                shiftY = playerBounds.min.y - bottom;
            }
            else if (playerBounds.max.y > top)
            {
                shiftY = playerBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;

            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
