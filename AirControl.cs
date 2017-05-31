using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class AirControl : MonoBehaviour {



	public float maxFuel;
	public float fuelUseRate;

	float fuelUseRateForward;
	float fuelUseRateDirectional;

	public float currentFuel;
	public ThirdPersonCharacter thirdPersonCharacter;
	public TargetControl targetControl;

	string dashState;
	public float dashStrengthForward;
	public float dashStrengthVertical;
	public float dashStrengthHorizontal;
	Rigidbody playerRb;
	Vector3 forward;
	Vector3 up;
	Vector3 down;
	Vector3 left;
	Vector3 right;
	public int maxRefuels;
	public int currentRefuels;

	public GameObject airJet;
	public GameObject normalForUpDown;
	ParticleSystem airBlast;


	Vector3 offset = new Vector3(0,0.897f,0);
	public GameObject player;

	public GUIText playerGui;
	public GUIText statusGui;
	public string statusString;

	float jetDuration;
	public float maxJetDuration;

	/*
	refine 
	dashStrength
	fuelUseRate 
	look of airJet (looks strange for directional dashes)
	remove the press a component on directional dashes?

	

	*/

	// Use this for initialization
	void Start () 
	{
		playerRb = GetComponent<Rigidbody>();
		forward = new Vector3(0,0,dashStrengthForward);
		up = new Vector3(0,dashStrengthVertical,0);
		down = new Vector3(0,-dashStrengthVertical/2,0);
		left = new Vector3(-dashStrengthHorizontal,0,0);
		right = new Vector3(dashStrengthHorizontal,0,0);
		airBlast = airJet.GetComponent<ParticleSystem>();
		airBlast.Play();
		airJet.SetActive(false);
		fuelUseRateForward = fuelUseRate*2;
		fuelUseRateDirectional = 25;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!thirdPersonCharacter.m_IsGrounded)
		{

			DetermineDashState();
			//Debug.Log("dashState is " + dashState);
			Dash();
			DashControl(jetDuration);

			if(targetControl.grappling & currentFuel >= fuelUseRate/4)
			{
				currentFuel -= fuelUseRate/4;
			}
		}
		else
		{
			dashState = "none";
			airJet.SetActive(false);
			jetDuration = 0.7f;

			if(CrossPlatformInputManager.GetButtonDown("Refuel"))
				{
					Refuel();
				}
		}	
		UpdateGui();
	}


	void DetermineDashState()
	{
		if(CrossPlatformInputManager.GetAxis("Vertical") == 1 & CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			dashState = "up";
		}
		else if(CrossPlatformInputManager.GetAxis("Vertical") == -1 & CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			dashState = "down";
		}
		else if(CrossPlatformInputManager.GetAxis("Horizontal") == -1 & CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			dashState = "left";
		}
		else if(CrossPlatformInputManager.GetAxis("Horizontal") == 1 & CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			dashState = "right";
		}
		else if(CrossPlatformInputManager.GetButtonDown("Jump"))
		{
			dashState = "forward";
		}

	}
	float Dash()
	{
		if(dashState == "forward" & CrossPlatformInputManager.GetButton("Jump") & currentFuel >= fuelUseRateForward)
		{
			playerRb.AddRelativeForce(forward);
			currentFuel -= fuelUseRateForward;
		
			Vector3 jetF = airJet.transform.position - (player.transform.position + offset);
			AimJet(jetF);
			jetDuration = maxJetDuration;
			return jetDuration;
		}

		//

		else if(dashState == "up" & CrossPlatformInputManager.GetButtonDown("Jump") & currentFuel >= fuelUseRateDirectional)
		{	
			Vector3 jetSide1 = (player.transform.position + offset) - airJet.transform.position;
			Vector3 jetSide2 = normalForUpDown.transform.position - airJet.transform.position;
			Vector3 jetU = Vector3.Cross(jetSide2,jetSide1);
			AimJet(jetU);

			playerRb.velocity += up;
			currentFuel -= fuelUseRateDirectional;
			jetDuration = maxJetDuration;
			return jetDuration;

		}
		else if(dashState == "down" & CrossPlatformInputManager.GetButtonDown("Jump") & currentFuel >= fuelUseRateDirectional)
		{

			Vector3 jetSide1 = (player.transform.position + offset) - airJet.transform.position;
			Vector3 jetSide2 = normalForUpDown.transform.position - airJet.transform.position;
			Vector3 jetD = Vector3.Cross(jetSide1,jetSide2);
			AimJet(jetD);

			playerRb.velocity += down;
			currentFuel -= fuelUseRateDirectional;

			jetDuration = maxJetDuration;
			return jetDuration;
		}
		else if(dashState == "left" & CrossPlatformInputManager.GetButtonDown("Jump") & currentFuel >= fuelUseRateDirectional)
		{

			Vector3 jetSide1 = (player.transform.position + offset) - airJet.transform.position;
			Vector3 jetSide2 = (player.transform.position + new Vector3(0,2,0)) - airJet.transform.position;
			Vector3 jetL = Vector3.Cross(jetSide2,jetSide1);
			AimJet(jetL);

			left = Vector3.Cross(jetSide1,jetSide2);
			left = left.normalized*dashStrengthHorizontal;

			playerRb.velocity += left;
			currentFuel -= fuelUseRateDirectional;
			jetDuration = maxJetDuration;
			return jetDuration;
		}
		else if(dashState == "right" & CrossPlatformInputManager.GetButtonDown("Jump") & currentFuel >= fuelUseRateDirectional)
		{

			Vector3 jetSide1 = (player.transform.position + offset) - airJet.transform.position;
			Vector3 jetSide2 = (player.transform.position + new Vector3(0,2,0)) - airJet.transform.position;
			Vector3 jetR = Vector3.Cross(jetSide1,jetSide2);
			AimJet(jetR);

			right = Vector3.Cross(jetSide2,jetSide1);
			right = right.normalized*dashStrengthHorizontal;

			playerRb.velocity += right;
			currentFuel -= fuelUseRateDirectional;
			jetDuration = maxJetDuration;
			return jetDuration;
		}
		else
		{
			dashState = "none";
			return jetDuration;

			if(currentFuel < fuelUseRate || currentFuel < fuelUseRateForward || currentFuel < fuelUseRateDirectional)
			{
				Debug.Log("You Must Refuel");
				statusString = "refuel";

			}
			else
			{
				statusString = "fine";
			}
		}
	}
	void DashControl(float jetTime)
	{
		if(jetTime < 0.8f)
		{
			airJet.SetActive(false);
		}
		else
		{
			

			airJet.SetActive(true);
			airBlast.Play();
			jetDuration -= Time.deltaTime;
			//Debug.Log(jetTime + "duration");
		}




	}
	void AimJet(Vector3 dir)
	{
		
		Quaternion rot = Quaternion.LookRotation(dir);
		airJet.transform.rotation = Quaternion.Slerp (airJet.transform.rotation, rot,1);

	}

	void Refuel()
	{
		if(currentRefuels >= 1)
		{
			currentFuel = maxFuel;
			currentRefuels -= 1;	
			statusString = "fine";
		}
		else
		{
			Debug.Log("You have no more compressed air");
			statusString = "noSpare";

		}
	}
	void UpdateGui()
	{
		playerGui.text = "Spare canisters " + currentRefuels + "  ---  Current fuel level is " + currentFuel + "/" + maxFuel;
	
		if(statusString == "noSpare")
		{
			statusGui.text = "You have no more compressed air";
		}
		else if(statusString == "refuel")
		{
			statusGui.text = "You do not have enough fuel for that (Press B while grounded to Refuel)";
		}
		else if(statusString == "fine")
		{
			statusGui.text = " ";
		}
	}

}
