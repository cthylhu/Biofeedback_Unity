using UnityEngine;
using System.Collections;

public class collectItem : MonoBehaviour {
	GameObject[] treeList;
    GameObject[] houseRoofList;
    GameObject[] houseBaseList;
    GameObject[] treelineList;
	float t = 0;
	bool collected = false;

	// Use this for initialization
	void Start () {
		t = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if ((Time.time - t) > 10 && !collected) {
			this.GetComponentInChildren<Renderer> ().enabled = true;
		}
	}

	void OnTriggerEnter(Collider other) {
		this.GetComponentInChildren<Renderer> ().enabled = false;
		collected = true;
		changeDayToNight ();

	}

	public void changeDayToNight () {
		treeList = GameObject.FindGameObjectsWithTag("tree");
		foreach (GameObject t in treeList)
		{
			t.GetComponent<Renderer>().material = Resources.Load("trees_night", typeof(Material)) as Material;
		}
		
		houseRoofList = GameObject.FindGameObjectsWithTag("house");
		foreach (GameObject h in houseRoofList)
		{
			h.GetComponent<Renderer>().material = Resources.Load("house_night_roof", typeof(Material)) as Material;
		}
		
		houseBaseList = GameObject.FindGameObjectsWithTag("houseBase");
		foreach (GameObject h in houseBaseList)
		{
			h.GetComponent<Renderer>().material = Resources.Load("house_night", typeof(Material)) as Material;
		}
		
		treelineList = GameObject.FindGameObjectsWithTag("treeline");
		foreach (GameObject t in treelineList)
		{
			t.GetComponent<Renderer>().material = Resources.Load("treeline_night", typeof(Material)) as Material;
		}
		GameObject.Find ("Skylightdown").GetComponent<Light> ().enabled = false;
		GameObject.Find ("Nightlight").GetComponent<Light> ().enabled = true;
		GameObject.Find("ground").GetComponent<Renderer>().material = Resources.Load("ground_night", typeof(Material)) as Material;
		GameObject.Find("sky").GetComponent<Renderer>().material = Resources.Load("background_night", typeof(Material)) as Material;
		
	}
}
