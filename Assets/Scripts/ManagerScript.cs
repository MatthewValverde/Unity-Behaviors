using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

// script for managin objects, targets and gameplay.
// attach to empty gameobject in scene or camera.

[AddComponentMenu ("Infinite Camera-Control/Mouse Orbit with zoom")]
public class ManagerScript : MonoBehaviour
{
	// Ui objects
	public GameObject completeText;
	public Text successCount;
	public Text failCount;
	public Text timerDisplay;
	public GameObject startButton;

	// Materials for swapping modes
	public Material objectMaterial;
	public Material objectMaterialOutlineOver;
	public Material targetMaterial;
	public Material targetMaterialOutlineOver;
	public Material targetMaterialOutlinePositive;
	public Material targetMaterialOutlineNegative;

	// Audio Sources
	public AudioSource successAudio;
	public AudioSource failAudio;
	public AudioSource completeAudio;

	// Camera speeds and target
	public Transform cameraOrbitTarget;
	public float cameraXSpeed = 6.0f;
	public float cameraYSpeed = 6.0f;
	public float cameraScrollSpeed = 6.0f;

	// speed that object moves to target
	public float objectMoveSpeedToTarget = 21.0f;

	// flag the ability to use Keyboards Keys to control camera movements.
	public bool enableCameraMovementsViaKeys = true;

	// GameObject temp holders
	private GameObject[] mMovableObjects;
	private GameObject mSelectedGameObject;
	private GameObject mSelectedTarget;

	// integers
	private int mGreenPoints;
	private int mRedPoints;
	private int mTotalOfTargets = 0;
	private int mTimerCounter = 0;

	// Game flags
	private bool mObjectMoving = false;
	private bool mMoveItBack;
	private bool mRunTimer = false;
	private bool mGameHasStarted = false;

	// Move variables for objects
	private float mStartTime;
	private float mJourneyLength;
	private Vector3 mStartTransform;
	private Vector3 mEndTransform;
	private Quaternion mRotateStartTransform;
	private Quaternion mRotateEndTransform;

	// Camera control variables
	private bool mIsActivated;
	private Vector3 mCameraStartPostion;
	private Vector3 mCameraStartRotation;
	private Vector3 mPosition;
	private float mMinX = -360.0f;
	private float mMaxX = 360.0f;
	private float mMinY = -45.0f;
	private float mMaxY = 45.0f;
	private float mSensX = 100.0f;
	private float mSensY = 100.0f;
	private float mRotationY = 0.0f;
	private float mRotationX = 0.0f;
	private float mZoomMin = 1.0f;
	private float mZoomMax = 30.0f;
	private float mDistance;
	private float mX = 0.0f;
	private float mY = 0.0f;
	private float mSpeed = 2.0f;

	// Use this for initialization
	void Start ()
	{
		// init camera control vars
		Vector3 angles = transform.eulerAngles;
		mX = angles.y;
		mY = angles.x;
		mCameraStartPostion = Camera.main.transform.position;
		mCameraStartRotation = Camera.main.transform.localEulerAngles;
		mRotationY = 0 - Camera.main.transform.localEulerAngles.x;

		// find all relative game objects of type (if it has ObjectScript attached to gameobject), and get total targets count. 
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
		// methods for moving and roatating the object to target position
		if (mObjectMoving) {
			float distCovered = (Time.time - mStartTime) * objectMoveSpeedToTarget; // altering the object speed
			float fracJourney = distCovered / mJourneyLength;
			mSelectedGameObject.transform.position = Vector3.Lerp (mStartTransform, mEndTransform, fracJourney);
			mSelectedGameObject.transform.rotation = Quaternion.Lerp (mRotateStartTransform, mRotateEndTransform, fracJourney);
			if (mSelectedGameObject.transform.position == mEndTransform) {
				if (mMoveItBack) {
					// move and rotate the object back if it doesn't match the type
					ResetMoveVariables (mEndTransform, mStartTransform);
					ResetRotateVariables (mRotateEndTransform, mRotateStartTransform);
					mMoveItBack = false;
				} else {
					mObjectMoving = false;

					// resets material after it stops moving
					// get ObjectScript that is attached to gameObject
					ObjectScript objectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
					Renderer rend = mSelectedGameObject.GetComponent<Renderer> ();
					rend.material = (objectScript.material != null) ? objectScript.material : objectMaterial;
					mSelectedGameObject = null;
				}
			}
		}
	}

	// function that starts the game on button click
	public void StartGame ()
	{
		// make sure scene is neutral when starting
		ResetScene ();

		// removing start button
		if (startButton != null) {
			startButton.SetActive (false);
		}

		// starting timer
		mRunTimer = true;
		Invoke ("RunTimer", 1.0f);
		mGameHasStarted = true;
	}

	// adding a highlite to the target object on mouse over
	public void ObjectOver (GameObject gameObjectOver)
	{
		// separating flags for clarity...
		if (!mGameHasStarted)
			return;

		if (mObjectMoving)
			return;

		// get ObjectScript that is attached to gameObject
		// changing material
		ObjectScript objectScript = gameObjectOver.GetComponent<ObjectScript> ();
		Renderer rend = gameObjectOver.GetComponent<Renderer> ();
		rend.material = (objectScript.materialOutlineOver != null) ? objectScript.materialOutlineOver : targetMaterialOutlineOver;
	}

	// removing object highlite on mouse out
	public void ObjectOut (GameObject gameObjectOut)
	{
		// separating flags for clarity...
		if (!mGameHasStarted)
			return;

		if (mObjectMoving)
			return;

		// changing material
		// get ObjectScript that is attached to gameObject
		ObjectScript objectScript = gameObjectOut.GetComponent<ObjectScript> ();
		Renderer rend = gameObjectOut.GetComponent<Renderer> ();
		rend.material = (objectScript.material != null) ? objectScript.material : targetMaterial;
	}

	// handling hit on target after movable object is choosen
	public void TargetHit (GameObject gameTargetHit)
	{
		// separating flags for clarity...
		if (!mGameHasStarted)
			return;

		if (mSelectedGameObject == null)
			return;

		if (mObjectMoving)
			return;

		// get ObjectScript that is attached to gameObject
		ObjectScript objectScript = gameTargetHit.GetComponent<ObjectScript> ();
		if (mSelectedTarget != null) {
			mSelectedTarget = null;
		}

		// storing gameObject as current object
		mSelectedTarget = gameTargetHit;

		// get ObjectScript that is attached to selected gameObject
		ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
		if (selectedGameObjectScript == null)
			return;

		selectedGameObjectScript.isHaloActive = false;

		// init mouse out on target 
		ObjectOut (gameTargetHit);

		if (selectedGameObjectScript.types == null || objectScript.types == null) {
			return;
		}

		// run thru the type and see if they match, if so declare the flag true
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
			// changing material
			mSelectedTarget.GetComponent<Renderer> ().material = (objectScript.materialOutlinePositive != null) ? objectScript.materialOutlinePositive : targetMaterialOutlinePositive;

			// mark it in the correct position
			selectedGameObjectScript.isInCorrectPosition = true;
			AddToSuccess ();
		} else {
			// it's bad, so move it back to it's original position
			mMoveItBack = true;

			// changing material
			mSelectedTarget.GetComponent<Renderer> ().material = (objectScript.materialOutlineNegative != null) ? objectScript.materialOutlineNegative : targetMaterialOutlineNegative;
			AddToFail ();
		}

		// reset move vars before you start move
		ResetMoveVariables (mSelectedGameObject.transform.position, mSelectedTarget.transform.position);
		ResetRotateVariables (mSelectedGameObject.transform.rotation, mSelectedTarget.transform.rotation);

		// start the object moving
		mObjectMoving = true;
	}

	// handling a hit a movable object
	public void ObjectHit (GameObject gameObjectHit)
	{
		// separating flags for clarity...
		if (!mGameHasStarted) {
			return;
		}
		if (mObjectMoving)
			return;

		// get ObjectScript that is attached to gameObject and if object is already in position
		ObjectScript objectScript = gameObjectHit.GetComponent<ObjectScript> ();
		if (objectScript.isInCorrectPosition) {
			return;
		}

		// changing material
		gameObjectHit.GetComponent<Renderer> ().material = (objectScript.materialOutlineOver != null) ? objectScript.materialOutlineOver : objectMaterialOutlineOver;

		// if object has not been selected
		if (!objectScript.isHaloActive) {
			if (mSelectedGameObject != null) {

				// get ObjectScript that is attached to gameObject
				ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
				selectedGameObjectScript.isHaloActive = false;

				// changing material
				mSelectedGameObject.GetComponent<Renderer> ().material = (selectedGameObjectScript.material != null) ? selectedGameObjectScript.material : objectMaterial;
			}

			// storing gameObject as current object
			mSelectedGameObject = gameObjectHit;
			objectScript.isHaloActive = true;
		} else {

			// if object has been selected
			if (mSelectedGameObject != null) {

				// get ObjectScript that is attached to gameObject
				ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();

				// changing material
				mSelectedGameObject.GetComponent<Renderer> ().material = (selectedGameObjectScript.material != null) ? selectedGameObjectScript.material : objectMaterial;
				selectedGameObjectScript.isHaloActive = false;
				mSelectedGameObject = null;
			}
		}
	}

	// reseting the object move variables prior to a move
	private void ResetMoveVariables (Vector3 start, Vector3 end)
	{
		mStartTransform = start;
		mEndTransform = end;
		mStartTime = Time.time;
		mJourneyLength = Vector3.Distance (mStartTransform, mEndTransform);
	}

	// reseting the object rotate variables prior to a rotate
	private void ResetRotateVariables (Quaternion start, Quaternion end)
	{
		mRotateStartTransform = start;
		mRotateEndTransform = end;
	}

	// Add points to success text, audio source and storage int
	private void AddToSuccess ()
	{
		if (successAudio != null) {
			successAudio.Play ();
		}

		// add success points and display
		mGreenPoints++;
		if (successCount != null) {
			successCount.text = mGreenPoints.ToString ();
		}
		TestForWinner ();
	}

	// Add points to fail text, audio source and storage int
	private void AddToFail ()
	{
		if (failAudio != null) {
			failAudio.Play ();
		}

		// add fail points and display
		mRedPoints++;
		if (failCount != null) {
			failCount.text = mRedPoints.ToString ();
		}
	}

	// testing for the winner after every object is places
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

	// running the timer, formating and displaying
	private void RunTimer ()
	{
		CancelInvoke ("RunTimer");

		if (mRunTimer) {
			mTimerCounter++;

			// format time
			int minutes = Mathf.FloorToInt (mTimerCounter / 60F);
			int seconds = Mathf.FloorToInt (mTimerCounter - minutes * 60);
			string niceTime = string.Format ("{0:0}:{1:00}", minutes, seconds);

			if (timerDisplay != null) {
				timerDisplay.text = niceTime;
			}
			Invoke ("RunTimer", 1.0f);
		}
	}

	// showing the start button and removing the complete text ui
	private void ShowStartButton ()
	{
		if (startButton != null) {
			startButton.SetActive (true);
		}

		if (completeText != null) {
			completeText.SetActive (false);
		}
	}

	// reseeting the scene when the start button is pushed.
	private void ResetScene ()
	{
		// reseting variables to their default state
		CancelInvoke ("ResetScene");
		mTimerCounter = 0;
		timerDisplay.text = "0:00";
		failCount.text = "0";
		successCount.text = "0";
		mGreenPoints = 0;
		mRedPoints = 0;

		// reseting camera to default position
		ResetCameraPosition();

		// reseting game objects to their positions and the materials for both target and movable object.
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

	// moving the camera with key commands
	private void MoveCamera ()
	{
		// Reset camera position
		if (Input.GetKey (KeyCode.R)) {
			ResetCameraPosition();
		}

		// a flag to allow or disallow camera movements with Keys
		if(!enableCameraMovementsViaKeys) return;

		if (Input.GetKey (KeyCode.A)) {
			Camera.main.transform.position += Vector3.right * mSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.D)) {
			Camera.main.transform.position += Vector3.left * mSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.W)) {
			Camera.main.transform.position += Vector3.forward * mSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.S)) {
			Camera.main.transform.position += Vector3.back * mSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.Q)) {
			Camera.main.transform.position += Vector3.up * mSpeed * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.E)) {
			Camera.main.transform.position += Vector3.down * mSpeed * Time.deltaTime;
		}	
	}

	// moving the camera with the Right Mouse Down
	void LateUpdate ()
	{
		if (!mGameHasStarted)
			return;

		// Pan the camera
		float moveSpeed = 0.5f;
		if ((Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetMouseButton (1)) {
			Camera.main.transform.Translate (Vector3.right * -Input.GetAxis ("Mouse X") * moveSpeed);
			Camera.main.transform.Translate (Camera.main.transform.up * -Input.GetAxis ("Mouse Y") * moveSpeed, Space.World);
			return;
		}

		MoveCamera ();

		// only update if the mousebutton is held down
		if (Input.GetMouseButtonDown (1)) {
			mIsActivated = true;
		} 

		// if mouse button is let UP then stop rotating camera
		if (Input.GetMouseButtonUp (1)) {
			mIsActivated = false;
		} 
			
		if (cameraOrbitTarget && mIsActivated) { 
			//  get the distance the mouse moved in the respective direction
			mX += Input.GetAxis ("Mouse X") * cameraXSpeed;
			mY -= Input.GetAxis ("Mouse Y") * cameraYSpeed;	 

			// when mouse moves left and right we actually rotate around local y axis	
			Camera.main.transform.RotateAround (cameraOrbitTarget.position, Camera.main.transform.up, mX);

			// when mouse moves up and down we actually rotate around the local x axis	
			Camera.main.transform.RotateAround (cameraOrbitTarget.position, Camera.main.transform.right, mY);

			// reset back to 0 so it doesn't continue to rotate while holding the button
			mX = 0;
			mY = 0; 	

		} else {		

			// see if mouse wheel is used 	
			if (Input.GetAxis ("Mouse ScrollWheel") != 0) {	

				// get the distance between camera and target
				mDistance = Vector3.Distance (Camera.main.transform.position, cameraOrbitTarget.position);	

				// get mouse wheel info to zoom in and out	
				mDistance = ZoomLimit (mDistance - Input.GetAxis ("Mouse ScrollWheel") * cameraScrollSpeed, mZoomMin, mZoomMax);

				// position the camera FORWARD the right distance towards target
				mPosition = -(Camera.main.transform.forward * mDistance) + cameraOrbitTarget.position;

				// move the camera
				Camera.main.transform.position = mPosition; 
			}
		}
	}

	// calculating the zoom limit for the camera
	public static float ZoomLimit (float dist, float min, float max)
	{
		if (dist < min)
			dist = min;

		if (dist > max)
			dist = max; 

		return dist;
	}

	// reseting camera to default position
	void ResetCameraPosition(){
		Camera.main.transform.position = mCameraStartPostion;
		Camera.main.transform.localEulerAngles = mCameraStartRotation;
	}
}