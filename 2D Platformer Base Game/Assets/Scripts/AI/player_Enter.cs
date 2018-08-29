using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_Enter : MonoBehaviour {

    public GameObject Stone;

    private void OnTriggerEnter2D()
    {
        Stone.SetActive(true);
    }
}
