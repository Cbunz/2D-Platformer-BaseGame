using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileControl : MonoBehaviour {

    public Vector2 dimensions = new Vector2(2, 1);
    public Vector2 center = new Vector2(0, 0);

    SpriteRenderer spriteRenderer;
    BoxCollider2D _collider;
    Damager damager;


	private void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        damager = GetComponent<Damager>();
	}
	
	private void Update ()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.size = dimensions;
        }
        if (_collider != null)
        {
            _collider.size =  new Vector2(dimensions.x, dimensions.y);
        }
        if (damager != null)
        {
            damager.size = dimensions;
        }

        //transform.position = (Vector2)transform.position + center;
	}
}
