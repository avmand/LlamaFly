using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour 
{

	private CanvasGroup fadeGroup;
	private float fadeInDuration = 1.75f;
	private bool gameStarted;

	public Transform arrow;
	private Transform playerT;
	public Objective obj;

	private void Start()
	{
		playerT = FindObjectOfType<PlayerMotor>().transform;
		//Load level
		SceneManager.LoadScene(Manager.Instance.currentLevel.ToString(), LoadSceneMode.Additive);

		// Grab then only CanvasGroup in the scene
		fadeGroup = FindObjectOfType<CanvasGroup>();

		// Start with a white screen, hide the logos
		fadeGroup.alpha = 1;
	}

	private void Update() 
	{
		if(obj!=null)
		{
			//if obj there, rotate arrow
			Vector3 dir = playerT.InverseTransformPoint(obj.getcurrring().position);
			float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
			a += 180;
			arrow.transform.localEulerAngles = new Vector3(0, 180, a);
		}
		if (Time.timeSinceLevelLoad <= fadeInDuration)
		{
			// Initial fade-in
			fadeGroup.alpha = 1 - (Time.timeSinceLevelLoad / fadeInDuration);
		}
		// If the initial fade-in is completed, and the game has not been started
		else if (!gameStarted)
		{
			// Ensure the fade is completly gone
			fadeGroup.alpha = 0;
			gameStarted = true;
		}
	}
	public void ExitScene()
	{
		SceneManager.LoadScene("Menu");
	}

	public void CompleteLevel()
	{
		// Complete the level, and save the progress
		SaveManager.Instance.CompleteLevel(Manager.Instance.currentLevel);
		
		// Focus level selection  when we return to Menu scene
		Manager.Instance.menuFocus = 1;

		ExitScene();
	}
}
