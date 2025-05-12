using UnityEngine;

public class PlayerTagSette : MonoBehaviour
{
    void Awake()
    {
        gameObject.tag = "Player"; // Force the Player tag on spawn
        Debug.Log("Player spawned with tag: " + gameObject.tag);
    }
}

