using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMotor : MonoBehaviour 
{
	private CharacterController controller;

	public float baseSpeed = 9.0f;		//metres/second
	public float rotSpeedX = 3.0f;		//x axis
	public float rotSpeedY = 1.5f;      //y axis

	private float deathTime;
	private float deathDuration; 
	public GameObject deathExplode;

	private void Start()
	{
		controller = GetComponent<CharacterController>();

		///create trial
		GameObject trail = Instantiate(Manager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]);
		//set trail as children of model
		trail.transform.SetParent(transform.GetChild(0));
		//Fix rotation issue
		trail.transform.localEulerAngles = Vector3.forward * -90f;
	}

	private void Update()
	{

		//if we die, we don't continue moves from moveVector
		if (deathTime != 0)
		{
			if (Time.time - deathTime > deathDuration)
			{
				SceneManager.LoadScene("Game");
			}
			return;
		}


		// Give player forward player velocity
		Vector3 moveVector = transform.forward * baseSpeed;		//goes forward based on transform

		// Gather player's input
		Vector3 inputs = Manager.Instance.GetPlayerInput();

		// Get the delta direction
		Vector3 yaw = inputs.x * transform.right * rotSpeedX * Time.deltaTime;	//which way you go
		Vector3 pitch = inputs.y * transform.up * rotSpeedY * Time.deltaTime;
		Vector3 dir = yaw + pitch;

		// Make sure we are limit player from doing a loop
		float maxX = Quaternion.LookRotation(moveVector + dir).eulerAngles.x;

		// If has not going to far up/down, add the direction to the moveVector
		if (maxX < 90 && maxX > 70 || maxX > 270 && maxX < 290)
		{
			// Too far, don't do anything
		}
		else
		{
			// Add the direction to the current move
			moveVector += dir;
			
			// Have the player face where he is going
			transform.rotation = Quaternion.LookRotation(moveVector);
		}

		// move him
		controller.Move(moveVector * Time.deltaTime);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//set death time
		deathTime = Time.time;
		GameObject goop = Instantiate(deathExplode) as GameObject;
		goop.transform.position = transform.position;

		//kill player
		transform.GetChild(0).gameObject.SetActive(false);
	}
}
