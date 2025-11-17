using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform Player;

    private float maxReachedX;

    void Start()
    {
        maxReachedX = transform.position.x;
    }

    void LateUpdate()
    {
        if (Player != null)
        {
            // 1. Comparamos la posición X actual del jugador con la m/áxima que hemos alcanzad
            maxReachedX = Mathf.Max(maxReachedX, Player.position.x);

            // 2. Establecemos la posición de la cámara
            Vector3 position = transform.position;
            position.x = maxReachedX; 
            transform.position = position;
        }
    }
}