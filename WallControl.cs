using UnityEngine;
using System.Collections;

public class WallControl : MonoBehaviour {


	//Design:
	//sphere trigger collider(circle?). check if wall. if wall check the velocity and speed. if directly forward 
	//or angle is small, under 45(30?, 20?) then Mantle wall. if approach is directly to the side 90(100 to 80?) have 
	//wall Plant where character can stay for a moment and jump or grapple out(no jet). in between those angle ranges 
	//is when wall Run gets triggered. also, check if grapple target point is within this range and speed is under 
	//something, then activate Repel code. 






	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
