using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UIControllersAndData.Units;
using UnityEngine.UI;

public class StatsBattle : MonoBehaviour {


	  private List<ExistedUnit> _deployedUnits;


	public List<ExistedUnit> DeployedUnits
	{
		get {
			if(_deployedUnits == null){
				_deployedUnits =  Enumerable.Range(1, GameUnitsSettings_Handler.s.unitsNo).Select(i => new ExistedUnit()).ToList(); // by Fernando 05/26/2020		
				return _deployedUnits;
			}else
				return _deployedUnits;

		}
		set => _deployedUnits = value;
	}

	private List<ExistedUnit> _availableUnits;


	public List<ExistedUnit> AvailableUnits
	{
		get {
			if(_availableUnits == null){
				_availableUnits =  Enumerable.Range(1, GameUnitsSettings_Handler.s.unitsNo).Select(i => new ExistedUnit()).ToList(); // by Fernando 05/26/2020		
				return _availableUnits;
			}else
				return _availableUnits;

		}		set => _availableUnits = value;
	}


	public bool tutorialBattleSeen = false; 

	public Slider goldBar, manaBar, crystalsBar;
	public Text goldLb, manaLb, crystalsLb, remainingUnitsNo;
	public Toggle soundToggle,ambientToggle,musicToggle;

	public float 
		gold = 0, 
		mana = 0;//increasing as map is attacked

	public int 
		crystals = 0, 
		unitsLost = 0, 
		buildingsDestroyed = 0,
		maxStorageGold = 0, //maximum loot existing on the map - necessary for progress bars
		maxStorageMana = 0, 
		maxCrystals = 0; 

	public GameObject GhostHelperBattle;
	private Component transData, soundFX;

	void Start () 
	{
		
		// _deployedUnits =  Enumerable.Range(1, GameUnitsSettings_Handler.s.unitsNo).Select(i => new ExistedUnit()).ToList(); // by Fernando 05/26/2020		
		// _availableUnits =  Enumerable.Range(1, GameUnitsSettings_Handler.s.unitsNo).Select(i => new ExistedUnit()).ToList(); // by Fernando 05/26/2020		
		transData = GameObject.Find("TransData").GetComponent<TransData>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();

		LoadTransData ();
		UpdateUI ();
	}

	private void LoadTransData()
	{
		tutorialBattleSeen = ((TransData)transData).tutorialBattleSeen;
		if (!tutorialBattleSeen) GhostHelperBattle.SetActive (true);

		AvailableUnits = ((TransData)transData).GoingToBattleUnits;
		

		if (!((TransData)transData).soundOn) 
		{			
			((SoundFX)soundFX).ChangeSound (((TransData)transData).soundOn);
//			soundToggle.Start ();
//			soundToggle.Set (false, false);
		}

		if (!((TransData)transData).ambientOn) 
		{
			((SoundFX)soundFX).ToggleAmbient ();
//			ambientToggle.Start ();
//			ambientToggle.Set (false, false);
		}

		if (!((TransData)transData).musicOn) 
		{
			((SoundFX)soundFX).ChangeMusic (((TransData)transData).musicOn);
//			musicToggle.Start ();
//			musicToggle.Set (false, false);
		}

	}

	public void ApplyMaxCaps()//cannot exceed storage+bought capacity
	{
		if (gold > maxStorageGold) { gold = maxStorageGold; }
		if (mana > maxStorageMana) { mana = maxStorageMana; }
	}

	public void UpdateUI()//updates numbers and progress bars
	{
		goldBar.maxValue = (float)maxStorageGold;
		goldBar.value = (float)gold/(float)maxStorageGold;
		manaBar.maxValue = (float)maxStorageMana;
		manaBar.value = (float)mana/(float)maxStorageMana;
		//crystalsBar.maxValue = (float)maxCrystals;
		//crystalsBar.value = (float)crystals/(float)maxCrystals;
			
		goldLb.text = ((int)gold).ToString ();
		manaLb.text = ((int)mana).ToString ();
		//crystalsLb.text = crystals.ToString ();
	}

	public void UpdateUnitsNo()
	{		
		int remainingUnits = 0;
		for (int i = 0; i <  AvailableUnits.Count; i++) 
		{
			remainingUnits += AvailableUnits[i].count;
		}		
		remainingUnitsNo.text = remainingUnits.ToString ();// update remaining units
	}

	public void ReturnHome()
	{
		Application.LoadLevel ("Game");
	}
}
