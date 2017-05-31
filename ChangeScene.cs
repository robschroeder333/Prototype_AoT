using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;
public class ChangeScene : MonoBehaviour {

	bool isStart;
	bool controls;
	bool testing;

	// Use this for initialization
	void Start () 
	{
		Scene scene = SceneManager.GetActiveScene ();

		if (scene.name == "StartScreen") {
			isStart = true;
		} 
		else if (scene.name == "Controls")
		{
			controls = true;
		}
		else
		{
			isStart = false;
			controls = false;
			testing = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (isStart && CrossPlatformInputManager.GetButtonDown ("Start")) 
		{
			SceneManager.LoadScene ("Test");
		}
		if (isStart && CrossPlatformInputManager.GetButtonDown ("Back")) 
		{
			SceneManager.LoadScene ("Controls");
		}
		if (controls && CrossPlatformInputManager.GetButtonDown("Back"))
		{
			SceneManager.LoadScene("StartScreen");
		}

	}
}
