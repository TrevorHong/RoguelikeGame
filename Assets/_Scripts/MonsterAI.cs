using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// https://youtu.be/uOobLo2y3KI
/// </summary>
public class MonsterAI : MonoBehaviour
{

    public string targetTag = "Player";

    private bool playerDetected = false;

    private Transform player;

    private IAstarAI ai;


    // tests stuff
    /*public int x = 0;
    public int y = 0;*/
    Vector3 point;

    // Start is called before the first frame update
    void Start()
    {
        ai = GetComponent<IAstarAI>();
    }

    // Update is called once per frame
    void Update()
    {
        /*float fallOffTime = 0f;
        if (playerDetected)
            fallOffTime = 30f;*/

        if (player  != null)
        {
            point.x = player.position.x;
            point.y = player.position.y;
        }

        //if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath))
        if (playerDetected)
        {
            Debug.Log("GOING TO PLAYER");
            ai.destination = point;
            ai.SearchPath();
            //fallOffTime -= Time.deltaTime;
        }
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Debug.Log("I DETECT PLAYER");
            playerDetected = true;
            player = collision.transform;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Debug.Log("LOST SIGHT OF PLAYER");
            playerDetected = false;
            player = null;
        }
    }
}
