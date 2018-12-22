using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallRespawn : MonoBehaviour {

    public float damageDelay = 0.5f;

    Transform player;
    Damageable playerDamageable;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        playerDamageable = player.GetComponent<Damageable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            PlayerCharacter.PlayerInstance.TriggerDieRespawn(false, true, 0);
            Invoke("TakeFallDamage", damageDelay);
        }
    }

    private void TakeFallDamage()
    {
        PlayerCharacter.PlayerInstance.damageable.SetHealth(playerDamageable.CurrentHealth - 1);
    }
}
