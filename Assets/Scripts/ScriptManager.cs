using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScriptManager : MonoBehaviour
{
	public GameObject haloBlue;
	public GameObject haloGreen;
	public GameObject haloRed;
	public GameObject haloYellow;
	public GameObject completeText;
	public Text successCount;
	public Text failCount;
	public Text timerDisplay;
	public GameObject startButton;
	public AudioSource successAudio;
	public AudioSource failAudio;
	public AudioSource completeAudio;
	public float objectMoveSpeed = 9.0f;

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

	// Use this for initialization
	void Start ()
	{
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
		if (mGameHasStarted) {
			MoveCamera ();
		}

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
					mSelectedGameObject = null;
				}
			}
		}
	}

	public void StartGame ()
	{
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

		ObjectScript objectScript = gameObjectOver.GetComponent<ObjectScript> ();
		if (objectScript.targetHasBeenHit)
			return;
		
		if (mHaloYellow == null) {
			mHaloYellow = GameObject.Instantiate (haloYellow);
		}
		mHaloYellow.transform.parent = gameObjectOver.transform;
		mHaloYellow.transform.position = gameObjectOver.transform.position;
		mHaloYellow.SetActive (true);
	}

	public void ObjectOut (GameObject gameObjectOut)
	{
		if (!mGameHasStarted)
			return;
		
		if (mHaloYellow != null) {
			mHaloYellow.SetActive (false);
		}
	}

	public void TargetHit (GameObject gameTargetHit)
	{
		if (!mGameHasStarted && mSelectedGameObject == null)
			return;
		
		ObjectScript objectScript = gameTargetHit.GetComponent<ObjectScript> ();
		if (mSelectedTarget != null) {
			ResetHalos ();
			mSelectedTarget = null;
		}

		mSelectedTarget = gameTargetHit;

		ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();	
		selectedGameObjectScript.isHaloActive = false;
		mHaloBlue.SetActive (false);
		ObjectOut (gameTargetHit);
		GameObject halo;

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
			if (mHaloGreen == null) {
				mHaloGreen = GameObject.Instantiate (haloGreen);
			}
			halo =	mHaloGreen;
			selectedGameObjectScript.isInCorrectPosition = true;
			AddToSuccess ();
		} else {
			mMoveItBack = true;
			if (mHaloRed == null) {
				mHaloRed = GameObject.Instantiate (haloRed);
			}
			halo = mHaloRed;
			AddToFail ();
		}

		halo.transform.parent = gameTargetHit.transform;
		halo.transform.position = gameTargetHit.transform.position;
		halo.SetActive (true);

		ResetMoveVariables (mSelectedGameObject.transform.position, mSelectedTarget.transform.position);
		ResetRotateVariables (mSelectedGameObject.transform.rotation, mSelectedTarget.transform.rotation);

		mObjectMoving = true;
	}

	public void ObjectHit (GameObject gameObjectHit)
	{
		if (!mGameHasStarted) {
			return;
		}
		
		ObjectScript objectScript = gameObjectHit.GetComponent<ObjectScript> ();
		if (objectScript.isInCorrectPosition || mObjectMoving) {
			return;
		}

		ResetHalos ();

		if (!objectScript.isHaloActive) {
			if (mHaloBlue == null) {
				mHaloBlue = GameObject.Instantiate (haloBlue);
			}
			mHaloBlue.transform.parent = gameObjectHit.transform;
			mHaloBlue.transform.position = gameObjectHit.transform.position;
			mHaloBlue.SetActive (true);

			if (mSelectedGameObject != null) {
				ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();
				selectedGameObjectScript.isHaloActive = false;
			}

			mSelectedGameObject = gameObjectHit;
		} else {
			mHaloBlue.SetActive (false);
			mSelectedGameObject = null;
		}
	}

	public void ResetHalos ()
	{
		if (mHaloGreen != null)
			mHaloGreen.SetActive (false);
		if (mHaloRed != null)
			mHaloRed.SetActive (false);
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
					Invoke ("ResetScene", 3.0f);
					mRunTimer = false;
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

			if (timerDisplay != null) {
				timerDisplay.text = mTimerCounter.ToString ();
			}
			Invoke ("RunTimer", 1.0f);
		}
	}

	private void ResetScene ()
	{
		CancelInvoke ("ResetScene");
		mTimerCounter = 0;
		timerDisplay.text = "0";
		failCount.text = "0";
		successCount.text = "0";
		mGameHasStarted = false;
		mGreenPoints = 0;
		mRedPoints = 0;
		Camera.main.transform.position = mCameraStartPostion;
		Camera.main.transform.localEulerAngles = mCameraStartRotation;

		ResetHalos ();

		if (startButton != null) {
			startButton.SetActive (true);
		}

		if (completeText != null) {
			completeText.SetActive (false);
		}

		GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject> ();
		for (int i = 0; i < gameObjects.Length; i++) {
			ObjectScript objScript = gameObjects [i].GetComponent<ObjectScript> ();
			if (objScript != null && !objScript.isTargetObject) {
				objScript.Reset ();
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

		if (Input.GetMouseButton (1)) {
			rotationX += Input.GetAxis ("Mouse X") * sensX * Time.deltaTime;
			mRotationY += Input.GetAxis ("Mouse Y") * sensY * Time.deltaTime;
			mRotationY = Mathf.Clamp (mRotationY, minY, maxY);
			Camera.main.transform.localEulerAngles = new Vector3 (-mRotationY, rotationX, 0);
		}
			
	}

}