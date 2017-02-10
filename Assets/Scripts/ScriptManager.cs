using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking.Match;


[AddComponentMenu ("Infinite Camera-Control/Mouse Orbit with zoom")]
public class ScriptManager : MonoBehaviour
{
	//public GameObject haloBlue;
	//public GameObject haloGreen;
	//public GameObject haloRed;
	//public GameObject haloYellow;
	public GameObject completeText;
	public Text successCount;
	public Text failCount;
	public Text timerDisplay;
	public GameObject startButton;
	public AudioSource successAudio;
	public AudioSource failAudio;
	public AudioSource completeAudio;

	public Material objectMaterial;
	public Material objectOutlineMaterial;

	public Material targetMaterial;
	public Material targetOutlineOverMaterial;
	public Material targetOutlineGreenMaterial;
	public Material targetOutlineRedMaterial;

	private GameObject mHaloBlue;
	private GameObject mHaloGreen;
	private GameObject mHaloRed;
	private GameObject mHaloYellow;
	private GameObject mSelectedGameObject;
	private GameObject mSelectedTarget;
	private bool mIsHaloActive = false;
	private int mGreenPoints;
	private int mRedPoints;

	private bool mObjectMoving = false;
	private float mStartTime;
	private float mJourneyLength;

	private Vector3 mCameraStartPostion;
	private Vector3 mCameraStartRotation;
	private Vector3 mStartTransform;
	private Vector3 mEndTransform;
	private Quaternion mRotateStartTransform;
	private Quaternion mRotateEndTransform;
	private bool mMoveItBack;
	private int mTotalOfTargets = 0;
	private bool mRunTimer = false;
	private int mTimerCounter = 0;
	private bool mGameHasStarted = false;
	private GameObject[] mMovableObjects;
	private float minX = -360.0f;
	private float maxX = 360.0f;

	private float minY = -45.0f;
	private float maxY = 45.0f;

	private float sensX = 100.0f;
	private float sensY = 100.0f;

	private float mRotationY = 0.0f;
	private float rotationX = 0.0f;

	public Transform cameraOrbitTarget;
	public float cameraXSpeed = 6.0f;
	public float cameraYSpeed = 6.0f;
	public float cameraScrollSpeed = 6.0f;

	float objectMoveSpeed = 21.0f;

	float zoomMin = 1.0f;

	float zoomMax = 30.0f;

	float distance;

	Vector3 position;

	bool isActivated;



	float x = 0.0f;

	float y = 0.0f;

	// Use this for initialization
	void Start ()
	{
		Vector3 angles = transform.eulerAngles;

		x = angles.y;

		y = angles.x;

		mCameraStartPostion = Camera.main.transform.position;
		mCameraStartRotation = Camera.main.transform.localEulerAngles;
		mRotationY = 0 - Camera.main.transform.localEulerAngles.x;
		GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject> ();
		for (int i = 0; i < gameObjects.Length; i++) {
			ObjectScript objScript = gameObjects [i].GetComponent<ObjectScript> ();
			if (objScript != null && objScript.isTargetObject) {
				mTotalOfTargets++;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		//	if (mGameHasStarted) {
		//MoveCamera ();
		//}

		if (mObjectMoving) {
			float distCovered = (Time.time - mStartTime) * objectMoveSpeed;
			float fracJourney = distCovered / mJourneyLength;
			mSelectedGameObject.transform.position = Vector3.Lerp (mStartTransform, mEndTransform, fracJourney);
			mSelectedGameObject.transform.rotation = Quaternion.Lerp (mRotateStartTransform, mRotateEndTransform, fracJourney);
			if (mSelectedGameObject.transform.position == mEndTransform) {
				if (mMoveItBack) {
					ResetMoveVariables (mEndTransform, mStartTransform);
					ResetRotateVariables (mRotateEndTransform, mRotateStartTransform);
					mMoveItBack = false;
				} else {
					mObjectMoving = false;
					mSelectedGameObject.GetComponent<Renderer> ().material = objectMaterial;
					//mSelectedTarget.GetComponent<Renderer> ().material = targetMaterial;
					mSelectedGameObject = null;
				}
			}
		}
	}

	public void StartGame ()
	{
		ResetScene();

		if (startButton != null) {
			startButton.SetActive (false);
		}
		mRunTimer = true;
		Invoke ("RunTimer", 1.0f);
		mGameHasStarted = true;
	}

	private void ResetMoveVariables (Vector3 start, Vector3 end)
	{
		mStartTransform = start;
		mEndTransform = end;
		mStartTime = Time.time;
		mJourneyLength = Vector3.Distance (mStartTransform, mEndTransform);
	}

	private void ResetRotateVariables (Quaternion start, Quaternion end)
	{
		mRotateStartTransform = start;
		mRotateEndTransform = end;
	}

	public void ObjectOver (GameObject gameObjectOver)
	{
		if (!mGameHasStarted)
			return;

		if (mObjectMoving)
			return;

		Renderer rend = gameObjectOver.GetComponent<Renderer> ();
		rend.material = targetOutlineOverMaterial;

		ObjectScript objectScript = gameObjectOver.GetComponent<ObjectScript> ();
		if (objectScript.targetHasBeenHit)
			return;
		
		/*if (mHaloYellow == null) {
			mHaloYellow = GameObject.Instantiate (haloYellow);
		}
		mHaloYellow.transform.parent = gameObjectOver.transform;
		mHaloYellow.transform.position = gameObjectOver.transform.position;
		mHaloYellow.SetActive (true);*/
	}

	public void ObjectOut (GameObject gameObjectOut)
	{
		if (!mGameHasStarted)
			return;

		if (mObjectMoving)
			return;

		Renderer rend = gameObjectOut.GetComponent<Renderer> ();
		rend.material = targetMaterial;

	}

	public void TargetHit (GameObject gameTargetHit)
	{
		if (!mGameHasStarted)
			return;

		if (mSelectedGameObject == null)
			return;

		if (mObjectMoving)
			return;
		
		ObjectScript objectScript = gameTargetHit.GetComponent<ObjectScript> ();
		if (mSelectedTarget != null) {
			//ResetHalos ();
			mSelectedTarget = null;
		}

		mSelectedTarget = gameTargetHit;

		ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
		if (selectedGameObjectScript == null)
			return;

		selectedGameObjectScript.isHaloActive = false;
		//	mHaloBlue.SetActive (false);
		ObjectOut (gameTargetHit);
		//GameObject halo;

		if (selectedGameObjectScript.types == null || objectScript.types == null) {
			return;
		}

		bool isGood = false;
		for (int i = 0; i < selectedGameObjectScript.types.Length; i++) {
			for (int j = 0; j < objectScript.types.Length; j++) {
				if (selectedGameObjectScript.types [i] == (objectScript.types [j])) {
					isGood = true;
					break;
				}
			}
		}

		if (isGood) {
			//if (mHaloGreen == null) {
			//mHaloGreen = GameObject.Instantiate (haloGreen);
			//	}
			//	halo =	mHaloGreen;

			mSelectedTarget.GetComponent<Renderer> ().material = targetOutlineGreenMaterial;
			selectedGameObjectScript.isInCorrectPosition = true;
			AddToSuccess ();
		} else {
			mMoveItBack = true;
			//if (mHaloRed == null) {
			//mHaloRed = GameObject.Instantiate (haloRed);
			//}
			//halo = mHaloRed;

			mSelectedTarget.GetComponent<Renderer> ().material = targetOutlineRedMaterial;
			AddToFail ();
		}

		//halo.transform.parent = gameTargetHit.transform;
		//halo.transform.position = gameTargetHit.transform.position;
		//halo.SetActive (true);

		ResetMoveVariables (mSelectedGameObject.transform.position, mSelectedTarget.transform.position);
		ResetRotateVariables (mSelectedGameObject.transform.rotation, mSelectedTarget.transform.rotation);

		mObjectMoving = true;
	}

	public void ObjectHit (GameObject gameObjectHit)
	{
		if (!mGameHasStarted) {
			return;
		}
			
		if (mObjectMoving)
			return;
		
		ObjectScript objectScript = gameObjectHit.GetComponent<ObjectScript> ();
		if (objectScript.isInCorrectPosition) {
			return;
		}

		gameObjectHit.GetComponent<Renderer> ().material = objectOutlineMaterial;

		//	ResetHalos ();

		if (!objectScript.isHaloActive) {
			//if (mHaloBlue == null) {
			//mHaloBlue = GameObject.Instantiate (haloBlue);
			//	}
			//mHaloBlue.transform.parent = gameObjectHit.transform;
			//mHaloBlue.transform.position = gameObjectHit.transform.position;
			//mHaloBlue.SetActive (true);

			if (mSelectedGameObject != null) {
				ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
				selectedGameObjectScript.isHaloActive = false;
				mSelectedGameObject.GetComponent<Renderer> ().material = objectMaterial;
			}

			mSelectedGameObject = gameObjectHit;

			objectScript.isHaloActive = true;
		} else {
			//	mHaloBlue.SetActive (false);
			if (mSelectedGameObject != null) {
				ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
				selectedGameObjectScript.isHaloActive = false;
				mSelectedGameObject.GetComponent<Renderer> ().material = objectMaterial;
				mSelectedGameObject = null;
			}
		}
	}

	public void ResetHalos ()
	{
		//	if (mHaloGreen != null)
		//mHaloGreen.SetActive (false);
		//if (mHaloRed != null)
		//mHaloRed.SetActive (false);
	}

	private void AddToSuccess ()
	{
		if (successAudio != null) {
			successAudio.Play ();
		}

		mGreenPoints++;
		if (successCount != null) {
			successCount.text = mGreenPoints.ToString ();
		}
		TestForWinner ();
	}

	private void AddToFail ()
	{
		if (failAudio != null) {
			failAudio.Play ();
		}

		mRedPoints++;
		if (failCount != null) {
			failCount.text = mRedPoints.ToString ();
		}
	}

	private void TestForWinner ()
	{
		if (mGreenPoints == mTotalOfTargets) {
			if (completeText != null) {
				completeText.SetActive (true);
				if (completeAudio != null) {
					completeAudio.Play ();
					Invoke ("ShowStartButton", 5.0f);
					mRunTimer = false;
					mGameHasStarted = false;
					CancelInvoke ("RunTimer");
				}
			}
		}
	}

	private void RunTimer ()
	{
		CancelInvoke ("RunTimer");

		if (mRunTimer) {
			mTimerCounter++;

			int minutes = Mathf.FloorToInt (mTimerCounter / 60F);
			int seconds = Mathf.FloorToInt (mTimerCounter - minutes * 60);
			string niceTime = string.Format ("{0:0}:{1:00}", minutes, seconds);

			if (timerDisplay != null) {
				timerDisplay.text = niceTime;
			}
			Invoke ("RunTimer", 1.0f);
		}
	}

	private void ShowStartButton ()
	{
		if (startButton != null) {
			startButton.SetActive (true);
		}

		if (completeText != null) {
			completeText.SetActive (false);
		}
	}

	private void ResetScene ()
	{
		CancelInvoke ("ResetScene");
		mTimerCounter = 0;
		timerDisplay.text = "0:00";
		failCount.text = "0";
		successCount.text = "0";
		mGreenPoints = 0;
		mRedPoints = 0;
		Camera.main.transform.position = mCameraStartPostion;
		Camera.main.transform.localEulerAngles = mCameraStartRotation;

		//ResetHalos ();

		GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject> ();
		for (int i = 0; i < gameObjects.Length; i++) {
			ObjectScript objScript = gameObjects [i].GetComponent<ObjectScript> ();
			if (objScript != null) {
				if (!objScript.isTargetObject) {
					objScript.Reset ();
					gameObjects [i].GetComponent<Renderer> ().material = objectMaterial;
				} else {
					gameObjects [i].GetComponent<Renderer> ().material = targetMaterial;
				}
			}
		}
	}

	private float speed = 2.0f;
	private float zoomSpeed = 2.0f;

	private void MoveCamera ()
	{
		if (Input.GetKey (KeyCode.A)) {
			Camera.main.transform.position += Vector3.right * speed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.D)) {
			Camera.main.transform.position += Vector3.left * speed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.W)) {
			Camera.main.transform.position += Vector3.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.S)) {
			Camera.main.transform.position += Vector3.back * speed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.Q)) {
			Camera.main.transform.position += Vector3.up * speed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.E)) {
			Camera.main.transform.position += Vector3.down * speed * Time.deltaTime;
		}

		/*if (Input.GetMouseButton (1)) {
			rotationX += Input.GetAxis ("Mouse X") * sensX * Time.deltaTime;
			mRotationY += Input.GetAxis ("Mouse Y") * sensY * Time.deltaTime;
			mRotationY = Mathf.Clamp (mRotationY, minY, maxY);
			Camera.main.transform.localEulerAngles = new Vector3 (-mRotationY, rotationX, 0);
		}*/			
	}


	void LateUpdate ()
	{
		if (!mGameHasStarted)
			return;

		MoveCamera ();

		// only update if the mousebutton is held down
		if (Input.GetMouseButtonDown (1)) {
			isActivated = true;
		} 

		// if mouse button is let UP then stop rotating camera
		if (Input.GetMouseButtonUp (1)) {
			isActivated = false;
		} 
			
		if (cameraOrbitTarget && isActivated) { 
			//  get the distance the mouse moved in the respective direction
			x += Input.GetAxis ("Mouse X") * cameraXSpeed;
			y -= Input.GetAxis ("Mouse Y") * cameraYSpeed;	 

			// when mouse moves left and right we actually rotate around local y axis	
			Camera.main.transform.RotateAround (cameraOrbitTarget.position, Camera.main.transform.up, x);

			// when mouse moves up and down we actually rotate around the local x axis	
			Camera.main.transform.RotateAround (cameraOrbitTarget.position, Camera.main.transform.right, y);

			// reset back to 0 so it doesn't continue to rotate while holding the button
			x = 0;
			y = 0; 	

		} else {		

			// see if mouse wheel is used 	
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {	

				// get the distance between camera and target
				distance = Vector3.Distance (Camera.main.transform.position, cameraOrbitTarget.position);	

				// get mouse wheel info to zoom in and out	
				distance = ZoomLimit (distance - Input.GetAxis ("Mouse ScrollWheel") * cameraScrollSpeed, zoomMin, zoomMax);

				// position the camera FORWARD the right distance towards target
				position = -(Camera.main.transform.forward * distance) + cameraOrbitTarget.position;

				// move the camera
				Camera.main.transform.position = position; 
			}
		}
	}

	public static float ZoomLimit (float dist, float min, float max)
	{

		if (dist < min)
			dist = min;

		if (dist > max)
			dist = max; 

		return dist;
	}
}