using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour 
{
	private CanvasGroup fadeGroup;
	private float loadTime;
	private float minimumLogoTime = 2.0f; // Min time of that scene

	public void Start() 
	{
		// Grab  CanvasGroup in the scene
		fadeGroup = FindObjectOfType<CanvasGroup>();

		// Start with a white screen
		fadeGroup.alpha = 1;

		// Pre load the game
		

		// Get timestamp of the completion time
		// if load time is there give it some buff for logo
		if (Time.time < minimumLogoTime)
			loadTime = minimumLogoTime;
		else 
			loadTime = Time.time;
	}

	private void Update () 
	{
		// FadeIn
		if (Time.time < minimumLogoTime)
		{
			fadeGroup.alpha = 1 - Time.time;
		}

		// FadeOut
		if (Time.time > minimumLogoTime && loadTime != 0)
		{
			fadeGroup.alpha = Time.time - minimumLogoTime;
			if (fadeGroup.alpha >= 1)
			{
				SceneManager.LoadScene("Menu");
			}
			
		}

	}

}
