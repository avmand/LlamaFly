using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void OnClickCallback(int currentIndex);  
public delegate void OnClickAdditionCallback(Transform Panel, int currentIndex);  
public class MenuScene : MonoBehaviour 
{
	private CanvasGroup fadeGroup;
	private float fadeInSpeed = 0.33f;	//  1/3 where 3 is seconds

	public Button tiltControlButton;
	public Color tiltControlEnabled;
	public Color tiltControlDisabled;

	public RectTransform menuContainer;
	public Transform levelPanel;
	public Transform trailPanel;
	public Transform colorPanel;
	
	public Text colorBuySetText;
	public Text trailBuySetText;
	public Text goldText;

	private MenuCamera menuCamera;

	private int[] colorCost = new int[] { 0, 5, 5, 10, 10, 10, 15, 15, 20, 20};
	private int[] trailCost = new int[] { 0, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
	private int selectedColorIndex;
	private int selectedTrailIndex;
	private int activeColorIndex;
	private int activeTrailIndex;

	private Vector3 desiredMenuPosition;

	private GameObject currentTrail;

	public AnimationCurve enteringLevelZoomCurve;
	private bool isEnteringLevel = false;
	private float zoomDuration = 3.0f;
	private float zoomTransition;

	private Texture previousTrail;
	private GameObject lastPreviewObject;
	public Transform trailPreviewObject;
	public RenderTexture trailPreviewTexture;
	


	private void Start() 
	{
		

		if (SystemInfo.supportsAccelerometer)
		{
			// Is it currently enabled
			tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled: tiltControlDisabled;
		}
		else
		{
			tiltControlButton.gameObject.SetActive(false);
		}


		// Find the only MenuCamera and assign it
		menuCamera = FindObjectOfType<MenuCamera>();

		SetCameraTo(Manager.Instance.menuFocus);

		// Tell our gold  text how much he should displaying
		UpdateGoldText();

		// Grab the only CanvasGroup in the scene
		fadeGroup = FindObjectOfType<CanvasGroup>();

		// Start with a white screen
		fadeGroup.alpha = 1;
		
		// Add button on-click events to shop button
		InitShop();

		// Add button on-click events to levels 
		InitLevel();

	}

    private void Update() 
	{
		// Fade-in
		fadeGroup.alpha = 1 - Time.timeSinceLevelLoad * fadeInSpeed;

		// Menu navigation
		menuContainer.anchoredPosition3D = Vector3.Lerp(menuContainer.anchoredPosition3D, desiredMenuPosition, 0.1f);

		// Entering level zoom
		if (isEnteringLevel)
		{
			// Add to the zoomTransition float
			zoomTransition += (1 / zoomDuration) * Time.deltaTime;

			// Change the scale, following the animation curve
			menuContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5, enteringLevelZoomCurve.Evaluate(zoomTransition));

			// Change the desired position of canvas, so it can follow the scale up
			// This zoom in the center
			Vector3 newDesiredPosition = desiredMenuPosition * 5;
			// This adds to spesific position of the level on the canvas
			RectTransform rt = levelPanel.GetChild(Manager.Instance.currentLevel).GetComponent<RectTransform>();
			newDesiredPosition -= rt.anchoredPosition3D * 5;

			// This line will override the previous position 
			menuContainer.anchoredPosition3D = Vector3.Lerp(desiredMenuPosition, newDesiredPosition, enteringLevelZoomCurve.Evaluate(zoomTransition));

			// Fade to white screen, this will override the first line of update
			fadeGroup.alpha = zoomTransition;

			if (zoomTransition >= 1)
			{
				SceneManager.LoadScene("Game");
			}
		}
	}

	private void NavigateTo(int menuIndex)
	{
		switch (menuIndex)
		{			
			// 0 && default case = Main Menu
			default:
			case 0:
				desiredMenuPosition = Vector3.zero;
				menuCamera.BackToMainMenu();
				break;
			// 1 = Level Menu
			case 1:
				desiredMenuPosition = Vector3.right * 1280;
				menuCamera.MoveToLevel();
				break;
			// 2  = Shop Menu
			case 2:
				desiredMenuPosition = Vector3.left * 1280;
				menuCamera.MoveToShop();
				break;
		}

	}

	private void SetCameraTo(int menuIndex)
	{

		Debug.Log(menuIndex);
		NavigateTo(menuIndex);
		menuContainer.anchoredPosition3D = desiredMenuPosition;
	}
	
	private void UpdateGoldText()
	{
		goldText.text = SaveManager.Instance.state.gold.ToString();
	}

    private void InitLevel()
    {
		AddListenerToButton(levelPanel, OnLevelSelect, LevelAdditionalCallback);
    }

    private void  InitShop()
    {
		AddListenerToButton(colorPanel, OnColorSelect, (Transform t, int currentIndex) => {
			Image img = t.GetComponent<Image>();
			img.color = SaveManager.Instance.IsColorOwned(currentIndex) ? 
						Manager.Instance.playerColors[currentIndex] : 
						Color.Lerp(Manager.Instance.playerColors[currentIndex], new Color(0,0,0,1), 0.25f) ;
		});
		
		AddListenerToButton(trailPanel, OnTrailSelect, (Transform t, int currentIndex) => {
			RawImage img = t.GetComponent<RawImage>();
			img.color = SaveManager.Instance.IsTrailOwned(currentIndex) ? Color.white: Color.grey;
		});

		// set the previous trail, to prevent bug when swaping later
		previousTrail = trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RawImage>().texture;

	
		// Set player pref's (color & trail)
		OnColorSelect(SaveManager.Instance.state.activeColor);
		SetColor(SaveManager.Instance.state.activeColor);
	
		OnTrailSelect(SaveManager.Instance.state.activeTrail);
		SetTrail(SaveManager.Instance.state.activeTrail);

		// Make the button bigger for selected items
		colorPanel.GetChild(SaveManager.Instance.state.activeColor).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;
		trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

		// Create the trail preview
		lastPreviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[SaveManager.Instance.state.activeTrail]) as GameObject;
		lastPreviewObject.transform.SetParent(trailPreviewObject);
		lastPreviewObject.transform.localPosition = Vector3.zero;
    }

	private void SetColor(int index)
	{
		// set the active index
		activeColorIndex = index;

		// Change the color on the player model
		Manager.Instance.playerMaterial.color = Manager.Instance.playerColors[index];

		// Change buy/set button text
		colorBuySetText.text = "Current";

		// Save to player pref's
		SaveManager.Instance.state.activeColor = activeColorIndex;
		SaveManager.Instance.Save();
	}
	
	private void SetTrail(int index)
	{
		// set the active index
		activeTrailIndex = index;

		// Change the trail on the player model
		if (currentTrail != null)
			Destroy(currentTrail);

		// Create the new trail
		currentTrail = Instantiate(Manager.Instance.playerTrails[index]) as GameObject;

		// Set current trail as children of object player
		currentTrail.transform.SetParent(GameObject.FindGameObjectWithTag("Player").transform);

		// Fix the scaling issues / rotation issues
		currentTrail.transform.localPosition = Vector3.zero;
		currentTrail.transform.localRotation = Quaternion.Euler(0,0,90);
		currentTrail.transform.localScale = Vector3.one * 0.01f;

		// Change buy/set button text
		trailBuySetText.text = "Current";

		// Save to player pref's
		SaveManager.Instance.state.activeTrail = activeTrailIndex;
		SaveManager.Instance.Save();
	}
	

    // For every children transform under our panel, find the button and add on click
    private void AddListenerToButton(Transform Panel, OnClickCallback Callback, OnClickAdditionCallback AdditionalCallback)
	{
		if (Panel == null) {
			Debug.Log("You did not assign panel in the inspector");
		}

		int i = 0;
		foreach (Transform t in Panel)
		{
			int currentIndex = i;

			Button b = t.GetComponent<Button>();
			b.onClick.AddListener(() => Callback(currentIndex));
			AdditionalCallback(t, currentIndex);
			i++;
		}
	}

    private void LevelAdditionalCallback(Transform t, int currentIndex)
    {
		Image img = t.GetComponent<Image>();
		Button b = t.GetComponent<Button>();

		// Is it unlocked?
		if (currentIndex <= SaveManager.Instance.state.completedLevel)
		{
			// it's unlocked!
			if (currentIndex == SaveManager.Instance.state.completedLevel)
			{
				// It's not completed
				img.color = Color.white;
			}
			else
			{	
				// It's completed
				img.color = Color.yellow;
			}
		}
		else
		{
			// Set to dark color
			img.color = Color.grey;

			// level is unlock, disable button
			b.interactable = false;
		}

    }

    private void OnColorSelect(int currentIndex) 
	{
		Debug.Log("Selecting color button : " + currentIndex);

		// if the button already selected exit
		if (selectedColorIndex == currentIndex)
			return;

		// Make the icon slighly bigger
		colorPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

		// Put the previous one on normal scale
		colorPanel.GetChild(selectedColorIndex).GetComponent<RectTransform>().localScale = Vector3.one;


		// Set the selected color
		selectedColorIndex = currentIndex;

		// Change the content of the buy/set button, depending on the state of the color 
		if (SaveManager.Instance.IsColorOwned(currentIndex)) 
		{
			// Color is owned 
			if (activeColorIndex == currentIndex)
			{
				colorBuySetText.text = "Current";
			}
			else
			{
				colorBuySetText.text = "Select";
			}
		}
		else
		{
			// color isn't owned
			colorBuySetText.text = "Buy: " + colorCost[currentIndex].ToString();
		}
	}

	private void OnTrailSelect(int currentIndex) 
	{
		Debug.Log("Selecting trail button : " + currentIndex);

		// if the button already selected exit
		if (selectedTrailIndex == currentIndex)
			return;

		// Preview Trail
		// Get the image of preview button 
		trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().texture = previousTrail;
		// Keep the new trail's preview image in previous trail
		previousTrail = trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture;
		// set the new trail preview image to other camera
		trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture = trailPreviewTexture;
		
		GameObject[] unusedTrails = GameObject.FindGameObjectsWithTag("Trail");
		foreach (GameObject unusedTrail in unusedTrails)
		{
			Destroy(unusedTrail);	
		}
		
		// Create the trail preview
		lastPreviewObject = GameObject.Instantiate(Manager.Instance.playerTrails[currentIndex]) as GameObject;
		lastPreviewObject.transform.SetParent(trailPreviewObject);
		lastPreviewObject.transform.localPosition = Vector3.zero;


		// Make the icon slighly bigger
		trailPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

		// Put the previous one on normal scale
		trailPanel.GetChild(selectedTrailIndex).GetComponent<RectTransform>().localScale = Vector3.one;

		// Set the selected color
		selectedTrailIndex = currentIndex;

		// Change the content of the buy/set button, depending on the state of the trail 
		if (SaveManager.Instance.IsTrailOwned(currentIndex)) 
		{
			// trail is owned 
			if (activeTrailIndex == currentIndex)
			{
				trailBuySetText.text = "Current";
			}
			else
			{
				trailBuySetText.text = "Select";
			}
		}
		else
		{
			// trail isn't owned
			trailBuySetText.text = "Buy: " + trailCost[currentIndex].ToString();
		}
	}

    private void OnLevelSelect(int currentIndex)
    {
		Manager.Instance.currentLevel = currentIndex;
		isEnteringLevel = true;
		Debug.Log("Selecting trail button : " + currentIndex);
    }

	public Preloader pre;
	public void OnReset()
	{
		Debug.Log("Reset Stats");
		SaveManager.Instance.ResetSave();
		//SaveManager.Instance.Save();
		//SaveManager.Instance.Load();
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		
		SaveManager.Instance.Awake();
		//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//SetCameraTo(0);
		SceneManager.LoadScene("Preloader");
		//NavigateTo(0); 

	}


	public void OnColorBuySet() 
	{
		Debug.Log("Buy/Set color");

		// Is the selected color owned
		if (SaveManager.Instance.IsColorOwned(selectedColorIndex))
		{
			// set the color
			SetColor(selectedColorIndex);
		}
		else
		{
			// buy color
			if (SaveManager.Instance.BuyColor(selectedColorIndex, colorCost[selectedColorIndex]))
			{
				// Success
				SetColor(selectedColorIndex);

				// Change the color of button
				colorPanel.GetChild(selectedColorIndex).GetComponent<Image>().color = Manager.Instance.playerColors[selectedColorIndex];

				// Update gold text
				UpdateGoldText();
			}
			else
			{
				// Do not have enough gold
				Debug.Log("Not enough gold");
			}
		}
	}

	public void OnTrailBuySet() 
	{
		Debug.Log("Buytrail");

		// Is the selected Trail owned
		if (SaveManager.Instance.IsTrailOwned(selectedTrailIndex))
		{
			// set the Trail
			SetTrail(selectedTrailIndex);
		}
		else
		{
			// buy Trail
			if (SaveManager.Instance.BuyTrail(selectedTrailIndex, trailCost[selectedTrailIndex]))
			{
				// Success
				SetTrail(selectedTrailIndex);
				
				// Change the trail of button
				trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().color = Color.white;
				
				// Update gold text
				UpdateGoldText();
			}
			else
			{
				// Do not have enough gold
				Debug.Log("Not enough gold");
			}
		}
	}

	public void OnPlayClick() 
	{
		NavigateTo(1);
		Debug.Log("Clicked play");
	}

	public void OnShopClick() 
	{
		NavigateTo(2);		
		Debug.Log("clicked shop");
	}
	public void OnBackClick() 
	{
		NavigateTo(0);
		Debug.Log("clicked back");
	}

	public void OnTiltControlClick()
	{
		// Toogle usingAccelerometer value
		SaveManager.Instance.state.usingAccelerometer = !SaveManager.Instance.state.usingAccelerometer;

		// Save the changing
		SaveManager.Instance.Save();

		// Change the color
		tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled: tiltControlDisabled;
	}
}
