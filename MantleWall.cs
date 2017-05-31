using UnityEngine;
using System.Collections;

public class MantleWall : MonoBehaviour {


	public GameObject mantleSource;
	Vector3 fwd;
	public RaycastHit[,] mantleRays = new RaycastHit[3,4];
	public float mantleCheckDistance;
	 public Vector3 leftHighPoint;
	 public Vector3 centerHighPoint;
	 public Vector3 rightHighPoint;
	public float charHeight;
	 public Vector3 leftLandingPoint;
	 public Vector3 centerLandingPoint;
	 public Vector3 rightLandingPoint;
	float[] lowestPoint = new float[3];
	int lowestIndex = 0;
	float minval = 10000;
	bool hasLandingPoint;
	public GameObject player;
	Rigidbody playerRb;
	Vector3 leftNormal;
	Vector3 centerNormal;
	Vector3 rightNormal;
	Vector3 positionNormal;
	Vector3 positionHit;
	bool canAnchor;
	public float mantleUpSpeed;
	public float mantleForwardSpeed;
	bool moveUpDone;
	bool moveUpActive;
	bool moveForwardDone;
	bool moveForwardActive;
	Vector3 forwardLandingPoint = Vector3.zero;
	bool canMantle;
	public bool mantleOn;

	/*

	BUG TESTING!!!!
	new bug:   if one column of rays doesnt hit(hanging off corner or approach swiftly at angle cast origin is "through") character gets frozen in place
	look at how the transforms within charController relate to each other

	must retool thirdPersonChar works (landing check, animation)

	use magnitude of rays left center right to check what angle the approach is
	or get angle based on normal from contact point
	
	*/




	// Use this for initialization
	void Start () 
	{
		playerRb = player.GetComponent<Rigidbody>();
		canMantle = false;
		moveUpActive = false;
		moveForwardActive = false;
		moveUpDone = false;
	}
	
	// Update is called once per frame
	void Update () 
	{

		CastRays();
		if(mantleOn)
		{
			FindHighPoints();
			FindLandingPoint();
			if(hasLandingPoint)
			{

				if(lowestPoint[lowestIndex] <= mantleSource.transform.transform.GetChild(0).GetChild(0).position.y && canAnchor != false)
				{
					//landingpoint^	
					AnchorPosition();
					playerRb.useGravity = false;
					playerRb.isKinematic = true;
					TurnToWall();
				}

				if(canMantle)
				{
					MoveUp();
					if(moveUpDone)
					{
						MoveForward();

					}	
				}



			}
			if(moveForwardDone)
			{
				ClearRays();
			}

			//Debug.Log("left = " + leftHighPoint + " || center = " + centerHighPoint + " || right = " + rightHighPoint);
		}

	}


	void CastRays()
	{
		//double nested "for" loops
		fwd = mantleSource.transform.TransformDirection(Vector3.forward);
		int initialCount = mantleSource.transform.childCount;
		int secondCount = mantleSource.transform.GetChild(0).childCount;
		for (int i=0;i<initialCount;i++)
		{
			for(int ii=0;ii<secondCount;ii++)
			{
				Vector3 origin = mantleSource.transform.transform.GetChild(i).GetChild(ii).position;
				Ray ray = new Ray(origin,fwd);
				RaycastHit hit;
				if(Physics.Raycast(ray,out hit,mantleCheckDistance))
				{
					mantleRays[i,ii] = hit;
					Debug.DrawLine(origin,hit.point,Color.yellow);
					canAnchor = true;
				}
				else
				{
					mantleRays[i,ii] = default(RaycastHit);

				}
			}
		}
	}

	void FindHighPoints()
	{
		int initialCount = mantleSource.transform.childCount;
		int secondCount = mantleSource.transform.GetChild(0).childCount;
		bool leftDone = false;
		bool centerDone = false;
		bool rightDone = false;



		for(int ii=0;ii<secondCount;ii++)
		{
			for(int i=0;i<initialCount;i++)
			{
				if(mantleRays[i,ii].collider != null)
				{
					if(i==0 && leftDone == false)
					{
						leftHighPoint = mantleRays[i,ii].point;
						leftNormal = mantleRays[i,ii].normal;
						leftDone = true;
					}
					if(i==1 && centerDone == false)
					{
						centerHighPoint = mantleRays[i,ii].point;
						centerNormal = mantleRays[i,ii].normal;
						centerDone = true;
					}
					if(i==2 && rightDone == false)
					{
						rightHighPoint = mantleRays[i,ii].point;
						rightNormal = mantleRays[i,ii].normal;
						rightDone = true;
					}
				}
			}
		}
	}

	void FindLandingPoint()
	{
		

		hasLandingPoint = false;
		Vector3 nullVector = Vector3.zero;

		leftLandingPoint = Vector3.zero;
		centerLandingPoint = Vector3.zero;
		rightLandingPoint = Vector3.zero;

		if(leftHighPoint != nullVector)
		{
			fwd = leftNormal*-1;
			Vector3 offset = fwd/1.6f;
			Vector3 leftVertPos = leftHighPoint + offset + new Vector3(0,charHeight,0);
			Ray ray = new Ray(leftVertPos,Vector3.down);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,charHeight*2))
			{
				leftLandingPoint = hit.point;
				Debug.DrawLine(leftVertPos,leftLandingPoint,Color.yellow);
			}
			else
			{
				leftLandingPoint = Vector3.zero;
			}
						
		}
		if(centerHighPoint != nullVector)
		{
			fwd = centerNormal*-1;
			Vector3 offset = fwd/1.6f;
			Vector3 centerVertPos = centerHighPoint + offset + new Vector3(0,charHeight,0);
			Ray ray = new Ray(centerVertPos,Vector3.down);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,charHeight*2))
			{
				centerLandingPoint = hit.point;
				Debug.DrawLine(centerVertPos,centerLandingPoint,Color.yellow);
			}
			else
			{
				centerLandingPoint = Vector3.zero;
			}
		}
		if(rightHighPoint != nullVector)
		{
			fwd = rightNormal*-1;
			Vector3 offset = fwd/1.6f;
			Vector3 rightVertPos = rightHighPoint + offset + new Vector3(0,charHeight,0);
			Ray ray = new Ray(rightVertPos,Vector3.down);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,charHeight*2))
			{
				rightLandingPoint = hit.point;
				Debug.DrawLine(rightVertPos,rightLandingPoint,Color.yellow);
			}
			else
			{
				rightLandingPoint = Vector3.zero;
			}
		}	
		lowestPoint[0] = leftLandingPoint.y;
		lowestPoint[1] = centerLandingPoint.y;
		lowestPoint[2] = rightLandingPoint.y;
		for(int i=0;i<3;i++)
		{
			
			if(lowestPoint[i] != 0)
			{
				if(lowestPoint[i] < minval)
				{
					minval = lowestPoint[i];
					lowestIndex = i;
				}

				hasLandingPoint = true;
			}

		}


	}
	void AnchorPosition()
	{
		//needs to move player to just before wall contact (use reverse of ray to consistently have the right distance))
		playerRb.velocity = Vector3.zero;
		if(canAnchor)
		{
			

			if(leftHighPoint != Vector3.zero && centerHighPoint != Vector3.zero && rightHighPoint != Vector3.zero)
			{
				positionHit = (leftHighPoint + centerHighPoint + rightHighPoint)/3;
				positionNormal = (leftNormal + centerNormal + rightNormal)/3;
			}
			else if(centerHighPoint != Vector3.zero)
			{
				positionHit = centerHighPoint;
				positionNormal = centerNormal;
			}
			else if(leftHighPoint != Vector3.zero && rightHighPoint != Vector3.zero)
			{
				positionHit = (leftHighPoint + rightHighPoint)/2;
				positionNormal = (leftNormal + rightNormal)/2;
			}
			else if(leftHighPoint != Vector3.zero)
			{
				positionHit = leftHighPoint;
				positionNormal = leftNormal;
			}
			else if(rightHighPoint != Vector3.zero)
			{
				positionHit = rightHighPoint;
				positionNormal = rightNormal;
			}
			//Debug.Log("normal " + positionNormal);
			Debug.DrawRay(positionHit,positionNormal,Color.green);
			Vector3 anchoredPos = positionHit + positionNormal/3;

			//Debug.Log("playerPos " + player.transform.position + " / anchor " + anchoredPos);

			player.transform.position = new Vector3(anchoredPos.x,player.transform.position.y,anchoredPos.z);
			canAnchor = false;
			canMantle = true;
		}



	}
	void TurnToWall()
	{
		Vector3 dir = Vector3.zero;

		if(lowestIndex == 0)
		{
			dir =  leftLandingPoint - leftHighPoint;
		}
		else if(lowestIndex == 1)
		{
			dir = centerLandingPoint - centerHighPoint;
		}
		else if(lowestIndex == 2)
		{
			dir = rightLandingPoint - rightHighPoint;
		}

		if(dir != Vector3.zero)
		{
			dir.y = 0;
			Quaternion rot = Quaternion.LookRotation(dir);
			player.transform.rotation = Quaternion.Slerp (player.transform.rotation, rot, 1);

		}
	}
	void MoveUp()
	{



		if(!moveUpDone)
		{
			
			if(lowestIndex == 0)
			{
				forwardLandingPoint = leftLandingPoint;
			}
			else if(lowestIndex == 1)
			{
				forwardLandingPoint = centerLandingPoint;
			}
			else if(lowestIndex == 2)
			{
				forwardLandingPoint = rightLandingPoint;
			}	
		}



		//lerp from current pos(anchor) to vertical distance from current to landing(plus vertOffset) +(charHeight/4)

		Vector3 vertFrom = player.transform.position;
		Vector3 vertTo = new Vector3(player.transform.position.x,lowestPoint[lowestIndex],player.transform.position.z);
		Vector3 velocity = Vector3.zero;
		//Debug.Log("vertTo " + vertTo);
		//Debug.Log("forwardActive " + moveForwardActive + " | " + "upActive " + moveUpActive);
		if(!moveForwardActive && !moveUpDone)
		{
			
			moveUpActive = true;
			player.transform.position = Vector3.SmoothDamp(vertFrom,vertTo,ref velocity,mantleUpSpeed);
			//player.transform.position = Vector3.Lerp(vertFrom,vertTo,mantleUpSpeed*Time.deltaTime);	
			if((player.transform.position - vertTo).magnitude < 0.3f)
			{
				
				player.transform.position = vertTo;
				moveUpDone = true;
				moveUpActive = false;
			}

		}

		//Debug.Log("moveUpDone " + moveUpDone);
		//check if movement is done in y-axis before moving forward\




	}
	void MoveForward()
	{


		moveForwardDone = false;


		//Debug.Log("forwardLandingPoint " + forwardLandingPoint);


		
		Vector3 forFrom = player.transform.position;
		Vector3 forTo = new Vector3(forwardLandingPoint.x,player.transform.position.y,forwardLandingPoint.z);
		//forTo = transform.TransformPoint(forTo);
		Vector3 velocity = Vector3.zero;


		if(!moveUpActive && !moveForwardDone)
		{
			
			moveForwardActive = true;
			player.transform.position = Vector3.SmoothDamp(forFrom,forTo,ref velocity,mantleForwardSpeed);
			if((player.transform.position - forTo).magnitude < 0.3f)
			{

				player.transform.position = forTo;
				moveForwardActive = false;
				moveForwardDone = true;
				canMantle = false;
				moveUpDone = false;
				playerRb.useGravity = true;
				playerRb.isKinematic = false;
			}

		}



	}

	void ClearRays()
	{
		int initialCount = mantleSource.transform.childCount;
		int secondCount = mantleSource.transform.GetChild(0).childCount;
		for (int i=0;i<initialCount;i++)
		{
			for(int ii=0;ii<secondCount;ii++)
			{
				mantleRays[i,ii] = default(RaycastHit);

			}
		}
		leftHighPoint = Vector3.zero;
		centerHighPoint = Vector3.zero;
		rightHighPoint = Vector3.zero;

	}

	void FindAnimation()
	{
		
	}

}
