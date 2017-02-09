using UnityEngine;
using System.Collections;

public class ScriptManager : MonoBehaviour
{
	public GameObject haloBlue;
	public GameObject haloGreen;
	public GameObject haloRed;
	public GameObject haloYellow;
	public float objectMoveSpeed = 0.5f;

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
	private Vector3 mStartTransform;
	private Vector3 mHoldTransform;
	private Vector3 mEndTransform;
	private bool mMoveItBack;

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		if (mObjectMoving) {
			float distCovered = (Time.time - mStartTime) * objectMoveSpeed;
			float fracJourney = distCovered / mJourneyLength;
			mSelectedGameObject.transform.position = Vector3.Lerp (mStartTransform, mEndTransform, fracJourney);
			if (mSelectedGameObject.transform.position == mEndTransform) {
				if (mMoveItBack) {
					ResetMoveVariables (mEndTransform, mStartTransform);
					mMoveItBack = false;
					print ("mMoveItBack ");
				} else {
					mObjectMoving = false;
					mSelectedGameObject = null;
					print ("finsihed moving");
				}
			}
		}
	}

	private void ResetMoveVariables (Vector3 start, Vector3 end)
	{
		mHoldTransform = start;
		mStartTransform = start;
		mEndTransform = end;
		mStartTime = Time.time;
		mJourneyLength = Vector3.Distance (mStartTransform, mEndTransform);
	}

	public void ObjectOver (GameObject gameObjectOver)
	{
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
		if (mHaloYellow != null) {
			mHaloYellow.SetActive (false);
		}
	}

	public void TargetHit (GameObject gameTargetHit)
	{
		if (mSelectedGameObject == null)
			return;
		
		ObjectScript objectScript = gameTargetHit.GetComponent<ObjectScript> ();
		//if (objectScript.targetHasBeenHit)
		//return;

		//objectScript.targetHasBeenHit = true;
		if (mSelectedTarget != null) {
			if (mHaloGreen != null)
				mHaloGreen.SetActive (false);
			if (mHaloRed != null)
				mHaloRed.SetActive (false);
			mSelectedTarget = null;
		}

		mSelectedTarget = gameTargetHit;

		ObjectScript selectedGameObjectScript = mSelectedGameObject.GetComponent<ObjectScript> ();	
		selectedGameObjectScript.isHaloActive = false;
		mHaloBlue.SetActive (false);
		//mSelectedGameObject.transform.position = gameTargetHit.transform.position;
		//mSelectedGameObject = null;

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
			mGreenPoints++;
		} else {
			mMoveItBack = true;
			if (mHaloRed == null) {
				mHaloRed = GameObject.Instantiate (haloRed);
			}
			halo = mHaloRed;
			mRedPoints++;
		}

		halo.transform.parent = gameTargetHit.transform;
		halo.transform.position = gameTargetHit.transform.position;
		halo.SetActive (true);

		ResetMoveVariables (mSelectedGameObject.transform.position, mSelectedTarget.transform.position);
		mObjectMoving = true;
	}

	public void ObjectHit (GameObject gameObjectHit)
	{
		if (mObjectMoving)
			return;
		
		ObjectScript objectScript = gameObjectHit.GetComponent<ObjectScript> ();
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

}
