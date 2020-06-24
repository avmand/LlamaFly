using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour 
{
	public static SaveManager Instance { get; set; }
	public SaveState state;
	public void Awake() 
	{
		DontDestroyOnLoad(gameObject);		
		if (Instance == null) Instance = this;	
		Load();

		// Are we using the accelerometer && can we use it
		if (state.usingAccelerometer && !SystemInfo.supportsAccelerometer)
		{
			// NO=>stop trying
			state.usingAccelerometer = false;
			Save();
		}
	}

	// Save state
	public void Save()
	{
		PlayerPrefs.SetString("save", Helper.Serialize<SaveState>(state));
	}

	// Reset save file
	public void ResetSave()
	{
		PlayerPrefs.DeleteKey("save");
	}

	// Load the previous saved state
	public void Load() 
	{
		
		if (PlayerPrefs.HasKey("save")) 
		{
			state = Helper.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
		}
		else
		{
			state = new SaveState();
			Save();
			Debug.Log("No save file found, creating a new one!");
		}
	}

	// can i buy tokens
	public bool BuyColor(int index, int cost)
	{
		if (state.gold >= cost)
		{
			// yes, pay up
			state.gold -= cost;
			UnlockColor(index);

			
			Save();
			return true;
		}
		else 
		{
			// No, you broke
			return false;
		}
	}
	// can i buy trail ////////////////////Should i implement
	public bool BuyTrail(int index, int cost)
	{
		if (state.gold >= cost)
		{
			//yes, pay up
			state.gold -= cost;
			UnlockTrail(index);

			Save();
			return true;
		}
		else 
		{
			// No
			return false;
		}
	}

	
	public bool IsColorOwned(int index) 
	{
		// Check if the bit is set, if so the color is owned
		return (state.colorOwned & (1 << index)) != 0;
	}

	public bool IsTrailOwned(int index) 
	{
		// Check if the bit is set, if so the trail is owned
		return (state.trailOwned & (1 << index)) != 0;
	}

	public void UnlockColor(int index)
	{
		// Toggle on the bit at index
		state.colorOwned |= 1 << index;
	}

	public void UnlockTrail(int index)
	{
		// Toggle on the bit at index
		state.trailOwned |= 1 << index;
	}

	public void CompleteLevel(int index)
	{
		// if this is the current active level
		if (state.completedLevel == index)
		{
			state.completedLevel++;
			Save();
		}
	}
}
