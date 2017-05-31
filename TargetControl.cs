using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class TargetControl : MonoBehaviour

{
    public float cursorSpeed = 1.0f;
	public GameObject cursorL;
	public GameObject cursorR;
    internal static bool visible;
    public float curDepth = 1.01f;
	public float screenEdgeX;// = 1.1f;
	public float screenEdgeY;// = 0.57f;

    public float grappleDistance = 150.0f;
    public RaycastHit leftHit;
    public RaycastHit rightHit;
    public float speed = 10.0f;
    GameObject player;
	Rigidbody playerRb;
	public float playerFloatSpeed;
	public ThirdPersonCharacter thirdPersonCharacter;
    ConfigurableJoint grapple;
    GameObject target;
	Rigidbody targetRb;
    public bool grappling;
    public Vector3 grappleFireOffset;
    bool fireL = false;
    bool fireR = false;
	public float gravNormal;
	public float gravFloat;
	public float lookSpeed;
	public float blendedMaxForce;
	public float normalMaxForce;
	Camera playerCamera;
	public GameObject cameraCorner;

	bool cursorLCanHit;
	bool cursorRCanHit;
	SpriteRenderer cursorLRend;
	SpriteRenderer cursorRRend;
	public Sprite cursorR_cannotHit;
	public Sprite cursorR_canHit;
	public Sprite cursorL_cannotHit;
	public Sprite cursorL_canHit;

	public GameObject leftGrapple;
	public GameObject rightGrapple;

	LineRenderer leftGrappleLine;
	LineRenderer rightGrappleLine;

	Vector3 lookDirection;
	Vector3 blendedTarget;
	Vector3 blendedLook;

	public AirControl airControl;



    //AGENDA
    /*
     	REFINEMENT
		camera turnspeed
		cursor movespeed
		grapple "feel" (adjust xdrive - reduce slightly? (only after look at target is done))
		max grapple distance
		gravity adjust
		lookAt speed
		maxForce values
		linear drag on character
		camera distance (should be closer?)
		cursor look(thicker lines)

		debug blended grapple issue ***
		find normal of left and right hit combined with blended and 0 y-axis. that will give me blended forward

		allow for moving up/down/left/right along wall?
		code for mantling a building if near the top (must write in thirdpersoncharacter script - racasts) 
		wallrun to maintain momentum
		code for blade control (use animator and create "sword" trigger collider)

	*/




    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
		playerRb = player.GetComponent<Rigidbody>();
        grapple = player.GetComponent<ConfigurableJoint>();
		target = grapple.connectedBody.gameObject;
		targetRb = target.GetComponent<Rigidbody> ();
        grappleFireOffset = new Vector3(0f, .7f, 0f);
        target.transform.position = player.transform.position + grappleFireOffset;
        grapple.anchor = grappleFireOffset;
        grappling = false;
		playerCamera = GetComponentInParent<Camera>();
		cursorLRend = cursorL.GetComponent<SpriteRenderer>();
		cursorRRend = cursorR.GetComponent<SpriteRenderer>();

		leftGrappleLine = leftGrapple.GetComponent<LineRenderer>();
		rightGrappleLine = rightGrapple.GetComponent<LineRenderer>();

    }
    void FixedUpdate()
    {
		

		if (grappling == false)
		{
			target.transform.position = player.transform.position + grappleFireOffset;
			targetRb.isKinematic = false;
			targetRb.useGravity = true;
		}


		//Debug.Log ("player speed is " + playerRb.velocity.magnitude);
		if (grappling && playerRb.velocity.magnitude > playerFloatSpeed) 
		{
			thirdPersonCharacter.m_GravityMultiplier = gravFloat;
		} 
		else if (grappling || playerRb.velocity.magnitude > playerFloatSpeed)
		{
			thirdPersonCharacter.m_GravityMultiplier = (gravFloat + 0.1f);
		}
		else
		{
			thirdPersonCharacter.m_GravityMultiplier = gravNormal;
		}

		HandleCursorStates();
    }
    // Update is called once per frame
    void Update()
    {
	    CursorControl();
		//HandleCursorStates();

        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            GrappleL();
			if (leftHit.collider != null) 
			{
				fireL = true;
			} 
			else 
			{
				fireL = false;
			}
        }
        if (CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            GrappleR();
			if (rightHit.collider != null) 
			{
				fireR = true;
			} 
			else 
			{
				fireR = false;
			}
        }

		if (fireL & fireR)
        {
            
            //Debug.Log("dual grapple at ");

			if (CrossPlatformInputManager.GetButton("Fire1") & CrossPlatformInputManager.GetButton("Fire2"))
            {
				Vector3 blendedTarget = target.transform.position;
				grappling = true;
				if(airControl.currentFuel >= airControl.fuelUseRate)
				{
					blendedTarget = Vector3.Lerp(leftHit.point, rightHit.point, 0.5f);

				}

				Vector3 playerGrapple = player.transform.position + grappleFireOffset;
				//Debug.Log("b" + blendedTarget + " t" + playerGrapple);
				if(blendedTarget == playerGrapple)
				{
					//cannot use != for some reason
				}
				else
				{
					DrawGrappleLine();
					DriveSwitchBlended();

					lookSpeed = 5f;

					LookToward(blendedTarget);
					MoveToGrapple(blendedTarget);
				}
                //Debug.Log("would have moved to" + blendedTarget);



            }
            else if (!CrossPlatformInputManager.GetButton("Fire1"))
            {
                fireL = false;
            }
            else if(!CrossPlatformInputManager.GetButton("Fire2"))
            {
                fireR = false;
            }

        }
		else if (fireL)
        {
            
            //Debug.Log("left grapple at " + leftHit.point);



			if (CrossPlatformInputManager.GetButton("Fire1") & airControl.currentFuel >= airControl.fuelUseRate)
            {
				grappling = true;
				DriveSwitchNormal();
				DrawGrappleLine();
				lookSpeed = 0.75f;
				LookToward(leftHit.point);
                MoveToGrapple(leftHit.point);
            }
            else
            {
				
                grappling = false;
                fireL = false;
				DrawGrappleLine();
                target.transform.position = player.transform.position + grappleFireOffset;
            }

        }
		else if (fireR)
        {
            
            //Debug.Log("right grapple at " + rightHit.point);
            


			if (CrossPlatformInputManager.GetButton("Fire2") & airControl.currentFuel >= airControl.fuelUseRate)
            {
				grappling = true;
				DriveSwitchNormal();
				DrawGrappleLine();
				lookSpeed = 0.75f;
				LookToward(rightHit.point);
                MoveToGrapple(rightHit.point);
            }
            else
            {

                grappling = false;
                fireR = false;
				DrawGrappleLine();
                target.transform.position = player.transform.position + grappleFireOffset;
            }
        }

    }
		






    void CursorControl()
    {
        float x = CrossPlatformInputManager.GetAxis("RstickX");
        float y = CrossPlatformInputManager.GetAxis("RstickY");
        float x2 = CrossPlatformInputManager.GetAxis("LstickX");
        float y2 = CrossPlatformInputManager.GetAxis("LstickY");

        float xL = cursorSpeed * x2;
        float yL = cursorSpeed * y2;
        float xR = cursorSpeed * x;
        float yR = cursorSpeed * y;

		cursorL.transform.localPosition = cursorL.transform.localPosition + (new Vector3(xL, yL, 0f) * Time.deltaTime);
		cursorR.transform.localPosition = cursorR.transform.localPosition + (new Vector3(xR, yR, 0f) * Time.deltaTime);


		Vector3 upperRightCorner = playerCamera.ViewportToWorldPoint(new Vector3(1,1, playerCamera.nearClipPlane + 1.1f));

		cameraCorner.transform.position = upperRightCorner;
		upperRightCorner = playerCamera.transform.InverseTransformPoint(upperRightCorner);
		screenEdgeX = upperRightCorner.x - 0.1f;
		screenEdgeY = upperRightCorner.y - 0.09f;

		Vector3 left = cursorL.transform.localPosition;
		left.x = Mathf.Clamp(cursorL.transform.localPosition.x, -screenEdgeX, screenEdgeX);
		left.y = Mathf.Clamp(cursorL.transform.localPosition.y, -screenEdgeY, screenEdgeY);
		left.z = Mathf.Clamp(cursorL.transform.localPosition.z, 1.1f, 1.1f);
		cursorL.transform.localPosition = left;
		Vector3 right = cursorR.transform.localPosition;
		right.x = Mathf.Clamp(cursorR.transform.localPosition.x, -screenEdgeX, screenEdgeX);
		right.y = Mathf.Clamp(cursorR.transform.localPosition.y, -screenEdgeY, screenEdgeY);
		right.z = Mathf.Clamp(cursorR.transform.localPosition.z, 1.1f, 1.1f);
		cursorR.transform.localPosition = right;
    }
    RaycastHit GrappleL()
    {
        leftHit = new RaycastHit();
        Vector3 origin = Camera.main.transform.position;
		Vector3 targetL = cursorL.transform.position - origin;
        RaycastHit hit;
        Ray ray = new Ray(origin, targetL);
        //int targetsMask = 1 << 8;
        //targetsMask = ~targetsMask;

		if (Physics.Raycast (ray, out hit, grappleDistance)) 
		{
			leftHit = hit;
		} 
        return leftHit;
    }

	void CursorLState()
	{
		//leftHit = new RaycastHit();
		Vector3 origin = Camera.main.transform.position;
		Vector3 targetL = cursorL.transform.position - origin;
		RaycastHit hit;
		Ray ray = new Ray(origin, targetL);
		//int targetsMask = 1 << 8;
		//targetsMask = ~targetsMask;

		if (Physics.Raycast (ray, out hit, grappleDistance)) 
		{
			cursorLCanHit = true;
		} 
		else
		{
			cursorLCanHit = false;
		}
	}
	
    RaycastHit GrappleR()
    {
        rightHit = new RaycastHit();
        Vector3 origin = Camera.main.transform.position;
		Vector3 targetR = cursorR.transform.position - origin;
        RaycastHit hit;
        Ray ray = new Ray(origin, targetR);
        //int targetsMask = 1 << 8;
        //targetsMask = ~targetsMask;

        if (Physics.Raycast(ray, out hit, grappleDistance))
        {
            rightHit = hit;
        }
        return rightHit;
    }

	void CursorRState()
	{
		//leftHit = new RaycastHit();
		Vector3 origin = Camera.main.transform.position;
		Vector3 targetR = cursorR.transform.position - origin;
		RaycastHit hit;
		Ray ray = new Ray(origin, targetR);
		//int targetsMask = 1 << 8;
		//targetsMask = ~targetsMask;

		if (Physics.Raycast (ray, out hit, grappleDistance)) 
		{
			cursorRCanHit = true;
		} 
		else
		{
			cursorRCanHit = false;
		}
	}

	void HandleCursorStates()
	{
		CursorLState();
		CursorRState();
		//Debug.Log("L state is " + cursorLCanHit + " and R state is " + cursorRCanHit);

		if(cursorLCanHit && cursorRCanHit)
		{
			
			cursorLRend.sprite = cursorL_canHit;
			cursorRRend.sprite = cursorR_canHit;
		}
		else if(cursorLCanHit & !cursorRCanHit)
		{
			
			cursorLRend.sprite = cursorL_canHit;
			cursorRRend.sprite = cursorR_cannotHit;
		
		}
		else if(cursorRCanHit & !cursorLCanHit)
		{
			
			cursorRRend.sprite = cursorR_canHit;
			cursorLRend.sprite = cursorL_cannotHit;
		}
		else
		{
			cursorLRend.sprite = cursorL_cannotHit;
			cursorRRend.sprite = cursorR_cannotHit;
		}
		
	}

    void MoveToGrapple(Vector3 newTarget)
    {
        
        target.transform.position = newTarget;

		targetRb.isKinematic = true;
		targetRb.useGravity = false;


    }

	void LookToward(Vector3 dir)
	{
		dir = dir - player.transform.position;
		if(dir != Vector3.zero)
		{
			dir.y = 0;
			Quaternion rot = Quaternion.LookRotation(dir);
			player.transform.rotation = Quaternion.Slerp (player.transform.rotation, rot, lookSpeed * Time.deltaTime);
				
		}

	}

	void DriveSwitchBlended()
	{
		JointDrive xz = new JointDrive();
		xz.maximumForce = blendedMaxForce;
		xz.positionSpring = 10;
		xz.positionDamper = 5;

		JointDrive y = new JointDrive();
		y.maximumForce = blendedMaxForce;
		y.positionSpring = 15;
		y.positionDamper = 5;

		grapple.xDrive = xz;
		grapple.yDrive = y;
		grapple.zDrive = xz;
	}

	void DriveSwitchNormal()
	{
		JointDrive xz = new JointDrive();
		xz.maximumForce = normalMaxForce;
		xz.positionSpring = 10;
		xz.positionDamper = 5;

		JointDrive y = new JointDrive();
		y.maximumForce = normalMaxForce;
		y.positionSpring = 15;
		y.positionDamper = 5;

		grapple.xDrive = xz;
		grapple.yDrive = y;
		grapple.zDrive = xz;
	}

	void DrawGrappleLine()
	{


		if(fireL)
		{
			Vector3 pointFrom = leftGrapple.transform.position;
			Vector3 pointTo = leftHit.point;

			Vector3[] points = new Vector3[2];
			points[0] = pointFrom;
			points[1] = pointTo;
			leftGrappleLine.SetPositions(points);
		}
		else
		{
			Vector3 pointFrom = leftGrapple.transform.position;
			Vector3 pointTo = leftGrapple.transform.position;

			Vector3[] points = new Vector3[2];
			points[0] = pointFrom;
			points[1] = pointTo;
			leftGrappleLine.SetPositions(points);
		}

		if(fireR)
		{
			Vector3 pointFrom = rightGrapple.transform.position;
			Vector3 pointTo = rightHit.point;

			Vector3[] points = new Vector3[2];
			points[0] = pointFrom;
			points[1] = pointTo;
			rightGrappleLine.SetPositions(points);
		}
		else
		{
			Vector3 pointFrom = rightGrapple.transform.position;
			Vector3 pointTo = rightGrapple.transform.position;

			Vector3[] points = new Vector3[2];
			points[0] = pointFrom;
			points[1] = pointTo;
			rightGrappleLine.SetPositions(points);
		}
			
	}

}
