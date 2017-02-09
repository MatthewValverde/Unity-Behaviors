using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ObjectScript : MonoBehaviour
{
	public string[] types;
	public bool isTargetObject = false;
	[HideInInspector] public bool isInCorrectPosition = false;
	[HideInInspector] public bool isHaloActive = false;
	[HideInInspector] public bool targetHasBeenHit = false;

	[HideInInspector] public Vector3 startPosition;
	[HideInInspector] public Quaternion startRotation;
	private GameObject mHaloObj;
	private ScriptManager mScriptManager;

	// Use this for initialization
	void Start ()
	{
		mScriptManager = GameObject.Find ("ScriptManagerObject").GetComponent<ScriptManager> ();
		startPosition = transform.position;
		startRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Reset ()
	{
		transform.position = startPosition;
		transform.rotation = startRotation;
		isHaloActive = false;
		targetHasBeenHit = false;
		isInCorrectPosition = false;
	}

	void OnMouseDown ()
	{
		if (isTargetObject) {
			mScriptManager.TargetHit (gameObject);
		} else {
			mScriptManager.ObjectHit (gameObject);
		}
		isHaloActive = !isHaloActive;
	}

	void OnMouseEnter ()
	{
		if (isTargetObject) {
			mScriptManager.ObjectOver (gameObject);
		}
	}

	void OnMouseExit ()
	{
		if (isTargetObject) {
			mScriptManager.ObjectOut (gameObject);
		}
	}
}