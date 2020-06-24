using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour 
{
	private Vector3 startwhereiamPosition;
	private Quaternion startwhereiamRotation;

	private Vector3 whereiwantposition;
	private Quaternion whereiwantrotation;

	public Transform shopWaypoint;
	public Transform levelWaypoint;

	// Use this for initialization
	private void Start () 
	{
		startwhereiamPosition = whereiwantposition = transform.localPosition;
		startwhereiamRotation = whereiwantrotation = transform.localRotation;
	}
	
	// Update is called once per frame
	private void Update () 
	{
		float x = Manager.Instance.GetPlayerInput().x;

		transform.localPosition = Vector3.Lerp(transform.localPosition, whereiwantposition + new Vector3(0,x,0) * 0.01f, 0.1f);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, whereiwantrotation, 0.1f);
	}

	public void BackToMainMenu()
	{
		whereiwantposition = startwhereiamPosition;
		whereiwantrotation = startwhereiamRotation;
	}

	public void MoveToShop()
	{
		whereiwantposition = shopWaypoint.localPosition;
		whereiwantrotation = shopWaypoint.localRotation;
	}

	public void MoveToLevel()
	{
		whereiwantposition = levelWaypoint.localPosition;
		whereiwantrotation = levelWaypoint.localRotation;
	}
}
