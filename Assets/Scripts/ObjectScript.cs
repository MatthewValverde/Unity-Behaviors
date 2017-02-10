using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// script for controlling objects and targets.
// attach to object or target.
public class ObjectScript : MonoBehaviour
{
	// sets a list of compatible object(s)
	public string[] types;

	// flags the object as target or not-- default false.
	public bool isTargetObject = false;

	// provide the object with it's own materials-- if not provided, the code falls back on the general materials set up in the Manager
	public Material material;
	public Material materialOutlineOver;
	public Material materialOutlinePositive;
	public Material materialOutlineNegative;

	// hidden public vars, just for communicating with Manager
	[HideInInspector] public bool isInCorrectPosition = false;
	[HideInInspector] public bool isHaloActive = false;
	[HideInInspector] public bool targetHasBeenHit = false;
	[HideInInspector] public Vector3 startPosition;
	[HideInInspector] public Quaternion startRotation;
	private GameObject mHaloObj;
	private ManagerScript mScriptManager;

	// Use this for initialization
	void Start ()
	{
		// get manager
		mScriptManager = GameObject.Find ("ManagerScriptObject").GetComponent<ManagerScript> ();

		// save start positions
		startPosition = transform.position;
		startRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	// reset object to it's original spot-- on game reset-- start button
	public void Reset ()
	{
		transform.position = startPosition;
		transform.rotation = startRotation;
		isHaloActive = false;
		targetHasBeenHit = false;
		isInCorrectPosition = false;
	}

	// send mouse down to manager
	void OnMouseDown ()
	{
		if (isTargetObject) {
			mScriptManager.TargetHit (gameObject);
		} else {
			mScriptManager.ObjectHit (gameObject);
		}
	}

	// send mouse over to manager
	void OnMouseEnter ()
	{
		if (isTargetObject) {
			mScriptManager.ObjectOver (gameObject);
		}
	}

	// send mouse out to manager
	void OnMouseExit ()
	{
		if (isTargetObject) {
			mScriptManager.ObjectOut (gameObject);
		}
	}
}