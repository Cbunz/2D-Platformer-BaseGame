using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Shield : MonoBehaviour
{
    // Shield Direction
    private PlayerCharacter player;
    [HideInInspector]
    public Transform shieldPivot;
    private Vector3 mousePos, screenPos;
    private Camera mainCamera;
    [HideInInspector]
    public Quaternion shieldDirection;
    [HideInInspector]
    public float shieldX, shieldY;
    [HideInInspector]
    public Vector2 boost = new Vector2();

    // Shield Boost
    public float boostAmountX, boostAmountY;

    private void Awake()
    {
        player = GetComponent<PlayerCharacter>();
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        shieldPivot = transform.Find("ShieldPivot");
    }

    private void Update()
    {
        if (shieldPivot != null)
            ShieldDirection();
    }

    private void ShieldDirection()
    {
        mousePos = Input.mousePosition;
        screenPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, (mainCamera.transform.position.z - mainCamera.transform.position.z)));
        shieldX = screenPos.x - player.transform.position.x;
        shieldY = screenPos.y - player.transform.position.y;
        boost = new Vector2(shieldX, shieldY).normalized;
        shieldDirection = Quaternion.Euler(0, 0, (Mathf.Atan2(shieldY, shieldX) * Mathf.Rad2Deg));
        shieldPivot.transform.rotation = shieldDirection;
    }
    
}
