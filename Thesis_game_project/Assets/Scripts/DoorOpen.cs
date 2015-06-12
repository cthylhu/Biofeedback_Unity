using UnityEngine;
using System.Collections;

public class DoorOpen : MonoBehaviour {
    public bool isDoorOpen = false;
    float startTime;

	void Update () {
        //this.transform.Translate(Vector3.left * Time.deltaTime);
	    if (isDoorOpen){
            float fracJourney = (Time.time - startTime) / 5f;
            transform.position = Vector3.Lerp(transform.position, new Vector3(-1.9f, 2f, 3.73f), fracJourney);    
        }
    }
	
    void OnTriggerEnter(Collider other) {
        if (!isDoorOpen){
            Debug.Log("Door Open!");
            //transform.Translate(Vector3.left * Time.deltaTime);
            //transform.position = new Vector3(-1.9f, 2f, 3.73f);
            isDoorOpen = true;
            startTime = Time.time;
        }
    }
}
