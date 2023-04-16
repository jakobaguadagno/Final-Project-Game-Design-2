using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowScript : MonoBehaviour
{
    public Vector3 targetPoint;
    public Vector3 startingPoint;
    public float flightDuration = 6f;

    private float startTime;
     
    private bool setVar = false;

    void Start()
    {
        Debug.Log("Arrow shoot!");
    }

    void Update()
    {
        if(targetPoint != null && startingPoint != null)
        {
            if(!setVar)
            {
                startTime = Time.time;
                setVar = true;
            }
            float timeElapsed = Time.time - startTime;
            if (timeElapsed >= flightDuration)
            {
                Destroy(gameObject);
                return;
            }
            float t = timeElapsed / flightDuration;
            Vector3 currentPosition = Vector3.Lerp(startingPoint, targetPoint, t);
            gameObject.transform.position = currentPosition;
            Vector3 direction = (targetPoint - startingPoint).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.Euler(0f, 0f, 180f);
        }
    }
}
