using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Menus;
using Assets.Scripts.UIControllersAndData;
using Assets.Scripts.UIControllersAndData.Store;
using UIControllersAndData;
using UIControllersAndData.Models;
using UIControllersAndData.Store;
using UIControllersAndData.Store.Categories.Unit;
using UIControllersAndData.Store.ShopItems.UnitItem;
using UIControllersAndData.Units;

//This script is active while the units menu is enabled on screen, then, the relevant info is passed to the unitProc

public class MenuUnit : MenuUnitBase
{

	public static MenuUnit Instance;

	private string _time;
	
	private int priceInCrystals = 0;

	private bool resetLabels = false;//the finish now upper right labels 
	public GameObject UnitProcObj; //target game obj for unit construction progress processor; disabled at start	


	private int 
		OffScreenY = 500,//Y positions ofscreen
		OnScreenY = 230;//action 0 cancel 1 finished 2 exitmenu

	private float z = -1f;

	public UnitProc unitProc;

	public List<UnitInfo> UnitsInfo { get; set; } = new List<UnitInfo>();


	private void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		trainingTimes = new int[GameUnitsSettings_Handler.s.unitsNo];
		sizePerUnit = new int[GameUnitsSettings_Handler.s.unitsNo];
		trainingIndexes = new int[GameUnitsSettings_Handler.s.unitsNo];

		UpdateData ();
	}

	

	private void UpdateData()
	{
		List<UnitCategoryLevels> unitCategoryLevels = ShopData.Instance.UnitCategoryData.category;
		List<UnitCategory> unitCategoryData = unitCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == 1).ToList();

		for (int i = 0; i < unitCategoryData.Count; i++)
		{
			trainingTimes[i] = unitCategoryData[i].TimeToBuild;
			sizePerUnit[i] = unitCategoryData[i].size;

			//in case user exits before passing the info to unit proc - MenuUnit is open
			unitProc.trainingTimes[i] = trainingTimes[i];			
			unitProc.sizePerUnit[i] = sizePerUnit[i];
		}
	}

	public void BuyStoreItem(UnitCategory itemData, Action callback)
	{
		
		if (itemData == null)
		{
			return;
		}
		
		rebuild = false;
		bool canBuild = true;

		if(itemData.Currency == CurrencyType.Gold)
		{
			if(!Stats.Instance.EnoughGold(itemData.Price))
			{				
				canBuild = false;
				MessageController.Instance.DisplayMessage("Insufficient gold.");				
			}
		}
		else if(itemData.Currency == CurrencyType.Mana)
		{
			if(!Stats.Instance.EnoughMana(itemData.Price))
			{
				canBuild = false;
				MessageController.Instance.DisplayMessage("Insufficient mana.");
			}
		}
		else
		{
			if(!Stats.Instance.EnoughCrystals(itemData.Price))
			{
				canBuild = false;
				MessageController.Instance.DisplayMessage("Insufficient crystals.");
			}
		}

		if(trainingIndexes[itemData.GetId()] == GameUnitsSettings_Handler.s.maxUnitsNo)
		{
			canBuild = false;
			MessageController.Instance.DisplayMessage(GameUnitsSettings_Handler.s.maxUnitsNo.ToString()+" units limit.");
		}

		if(Stats.Instance.occupiedHousing + itemData.size>(Stats.Instance.maxHousing))
		{
			canBuild = false;
			MessageController.Instance.DisplayMessage("Increase your soldier housing capacity.");
		}

		if (canBuild) 
		{			
			if(itemData.Currency == CurrencyType.Gold)
			{	
				Pay (itemData.Price, 0, 0); 
			}
			else if(itemData.Currency == CurrencyType.Mana)
			{
				Pay (0, itemData.Price, 0); 
			}
			else
			{
				Stats.Instance.crystals -= itemData.Price;
				Pay (0, 0, itemData.Price); 
			}

			Stats.Instance.experience += itemData.XpAward;
			if(Stats.Instance.experience>Stats.Instance.maxExperience)
				Stats.Instance.experience=Stats.Instance.maxExperience;

			Stats.Instance.occupiedHousing += itemData.size;

			Stats.Instance.UpdateUI();
			callback();
			Build (itemData.GetId());

			//AddUnitInfo(itemData.id, itemData.level);
		}
	}

	
	
	private void Pay(int gold, int mana, int crystals)
	{
		Stats.Instance.SubstractResources (gold, mana, crystals); 
	}
	private void Refund(int gold, int mana, int crystals)
	{
		Stats.Instance.AddResources (gold, mana, crystals); 
	}
	public void PassValuestoProc()
	{	
		pause = true;
		unitProc.Pause ();

		bool queEmpty = true;	//verify if there's anything under constuction
		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{
			if(trainingIndexes[i]>0)
			{
				queEmpty = false;
				break;
			}
		}
				
		if(!queEmpty)
		{			
			unitProc.currentSlidVal = currentSlidVal;	
			unitProc.currentTrainingTime = currentTrainingTime;			
			unitProc.queList.Clear();	//clear queIndex/trainingIndex/objIndex dictionary
																											
			for (int i = 0; i < trainingIndexes.Length; i++) 
			{						
				if(trainingIndexes[i]>0)
				{
					int index = ShopController.Intance.ListOfUnitStatusItem.FindIndex(x => x.ItemData.GetId() == i);
					
					unitProc.queList.Add(new Vector4(
					ShopController.Intance.ListOfUnitStatusItem[index].QIndex.Qindex,
					ShopController.Intance.ListOfUnitStatusItem[index].QIndex.Objindex, 
					trainingIndexes[i],
					ShopController.Intance.ListOfUnitStatusItem[index].Level));
				}

			}
			unitProc.trainingTimes = trainingTimes;
			unitProc.SortList();
			EraseValues();
		}
		unitProc.sizePerUnit = sizePerUnit;//pass the weights regardless
		Stats.Instance.sizePerUnit = sizePerUnit;

		unitProc.Resume ();
	}
	private void EraseValues()
	{		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{			
			if(trainingIndexes[i]>0)
			{
				int a = trainingIndexes[i];		//while unbuilding, trainingIndexes[i] is modified - no longer valid references
				for (int j = 0; j < a; j++)
				{
					UnBuild(i,2);
				} 
			}
		}
		currentSlidVal = 0;
		timeRemaining = 0;
		currentTimeRemaining = 0;
		hours = minutes = seconds = 0 ; //?totalTime
		queList.Clear ();	
		ShopController.Intance.UpdateHitText("Tap on a unit to summon them and read the description.");
			
	}
		
	public void LoadValuesfromProc()
	{	
		unitProc.Pause ();

		pause = true;

		bool queEmpty = true;

		if(unitProc.queList.Count > 0){queEmpty = false;}//unit proc is disabled at start???
	
		if(!queEmpty)
		{					
			currentSlidVal = unitProc.currentSlidVal;
			currentTrainingTime = unitProc.currentTrainingTime;

			queList.Clear();
			
			for (int i = 0; i < unitProc.queList.Count; i++) 
			{					
				queList.Add(unitProc.queList[i]);	
			}
			
			unitProc.queList.Clear();	//reset remote list
			ReBuild();
		}
		pause = false;
	}
		
	private void ReBuild()
	{		
		rebuild = true;

		queList.Sort(delegate (Vector4 v1, Vector4 v2)// qIndex, objIndex, trainingIndex
		{
			return v1.x.CompareTo(v2.x);			
		});		
		
		for (int i = 0; i < queList.Count; i++) // qIndex, objIndex, trainingIndex
		{			
			for (int j = 0; j < queList[i].z; j++) 
				{
					ShopController.Intance.AddStatusUnitFromSave((int)queList[i].y);
					Build((int)queList[i].y);
				}
		}

		progCounter = 0;	//delay first bar update 
		int index = ShopController.Intance.ListOfUnitStatusItem.FindIndex(x => x.ItemData.GetId() == (int) queList[0].y);
		ShopController.Intance.ListOfUnitStatusItem[index].Slider.value = currentSlidVal;
		
		UnitProcObj.SetActive(false);
		UpdateTime ();
	}

	void FixedUpdate()
	{
		if (pause)
			return;
		if(queCounter>0)
		{
			ProgressBars();	//fix this - progress bars resets currentSlidVal at reload
		}		
		else if(resetLabels)
		{				
			_time = "-";
			ShopController.Intance.UpdateUnitStatusData("-", "-");
			
			currentSlidVal = 0; progCounter = 0;
			resetLabels = false;			
		}
	}
			
	void Build(int id)
	{
		var levels = ShopData.Instance.GetLevels(id, ShopCategoryType.Unit);

		var unit = levels?.FirstOrDefault(x => ((ILevel) x).GetLevel() == 1);
		if (unit == null)
		{
			throw new Exception("Unity is null");
		}

		int i = ShopController.Intance.ListOfUnitStatusItem.FindIndex(x => x.ItemData.GetId() == id);
		resetLabels = true;
		bool iInQue = ShopController.Intance.ListOfUnitStatusItem[i].QIndex.inque;

		if(iInQue)
		{
			trainingIndexes[id] ++;
			ShopController.Intance.UpdateUnitsCountAndProgess(id, trainingIndexes[id]);
			ShopController.Intance.UpdateHitText(unit.Description);
			
		}			
		
		else if(!iInQue )
		{	
			trainingIndexes[id]++;
			ShopController.Intance.ListOfUnitStatusItem[i].QIndex.inque = true;
			ShopController.Intance.ListOfUnitStatusItem[i].QIndex.Qindex = queCounter;
			
			queCounter++;

			ShopController.Intance.UpdateUnitsCountAndProgess(id, trainingIndexes[id]);
			ShopController.Intance.UpdateHitText(unit.Description);
		}

		UpdateTime ();		
	}

	public void UnbuildUnit(UnitCategory itemData)
	{
		UnBuild(itemData.GetId(), 0);
	}
	
	void UnBuild(int id, int action)			// action 0 cancel 1 finished 2 exitmenu
	{
		int i = ShopController.Intance.ListOfUnitStatusItem.FindIndex(x => x.ItemData.GetId() == id);
		var item = ShopController.Intance.ListOfUnitStatusItem.Find(x => x.ItemData.GetId() == id);
		if (item == null)
		{
			return;
		}
		if(action == 0)
		{
			hours = minutes = seconds = 0;
			int 
				itemPrice = item.ItemData.Price;

			if(item.ItemData.Currency == CurrencyType.Gold)//return value is max storage capacity allows it
			{
				if (itemPrice < Stats.Instance.maxGold - (int)Stats.Instance.gold)
					Refund (itemPrice, 0, 0);
				else
				{
					Refund (Stats.Instance.maxGold - Stats.Instance.gold, 0, 0);//refunds to max storag capacity
					MessageController.Instance.DisplayMessage("Stop canceling units!\nYou are losing gold!");
				}
			}
			
			else if(item.ItemData.Currency == CurrencyType.Mana)
			{
				if(itemPrice<(Stats.Instance.maxMana - (int)Stats.Instance.mana))
					Refund (0, itemPrice, 0);
				else
				{
					Refund (0, Stats.Instance.maxMana - Stats.Instance.mana, 0);
					MessageController.Instance.DisplayMessage("Stop canceling units!\nYou are losing mana!");
				}
			}
			else
			{			
				Refund (0, 0, itemPrice);	
			}

			Stats.Instance.occupiedHousing -= item.ItemData.size;
			Stats.Instance.UpdateUI();
		}

		
		
		if(trainingIndexes[id] > 1)
		{
			trainingIndexes[id]--;

			ShopController.Intance.ListOfUnitStatusItem[i].Slider.value = 0;
			ShopController.Intance.UpdateUnitsCountAndProgess(id, trainingIndexes[id], 0.0f);
		}
		else
		{		
			ShopController.Intance.ListOfUnitStatusItem[i].QIndex.inque = false;
			ShopController.Intance.ListOfUnitStatusItem[i].QIndex.Qindex = 50;
			ShopController.Intance.ListOfUnitStatusItem[i].Slider.value = 0;
				
			queCounter--;
			trainingIndexes[id]--;
			ShopController.Intance.RemoveStatusItemFromList(i);

		}
		
		switch (action) 
		{
			case 0:
				ShopController.Intance.UpdateHitText("Training canceled.");
				break;
			case 1:
				ShopController.Intance.UpdateHitText("Training complete.");
				break;			
		}	

		UpdateTime ();
	}

	private void UpdateTime()
	{
		timeRemaining = 0;
		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{
			timeRemaining += trainingIndexes[i]*trainingTimes[i];
		}
		if(ShopController.Intance.ListOfUnitStatusItem.Count > 0)
		{
			currentTrainingTime = trainingTimes[ShopController.Intance.ListOfUnitStatusItem[0].QIndex.Objindex];
		}
		else
		{
			currentTrainingTime = 0;
		}
		timeRemaining -= currentSlidVal*currentTrainingTime;

		if(timeRemaining>0)
		{
			hours = (int)timeRemaining/60;
			minutes = (int)timeRemaining%60;
			seconds = (int)(60 - (currentSlidVal*currentTrainingTime*60)%60);			
		}

		if (minutes==60) minutes=0;
		if (seconds==60) seconds=0;

		if(hours>0 )
		{			
			_time = hours.ToString() +" h " + minutes.ToString() +" m " + seconds.ToString() +" s ";
		}
		else if(minutes > 0 )
		{		
			_time = minutes.ToString() +" m " + seconds.ToString() +" s ";
		}
		else if(seconds > 0 )
		{
			_time = seconds.ToString() +" s ";
		}
		
		if (timeRemaining >= 4320)	priceInCrystals = 150;
		else if (timeRemaining >= 2880)	priceInCrystals = 70;
		else if (timeRemaining >= 1440)	priceInCrystals = 45;
		else if (timeRemaining >= 600)	priceInCrystals = 30;
		else if (timeRemaining >= 180)	priceInCrystals = 15;
		else if (timeRemaining >= 60)	priceInCrystals = 7;
		else if (timeRemaining >= 30)	priceInCrystals = 3;
		else if (timeRemaining >= 0)	priceInCrystals = 1;

		ShopController.Intance.UpdateUnitStatusData(_time, priceInCrystals.ToString());

	}

	private void ProgressBars()
	{
		//Time.deltaTime = 0.016; 60*Time.deltaTime = 1s ; runs at 60fps

		progCounter += Time.deltaTime*0.5f;
		if(progCounter > progTime)
		{						
			int objIndex = ShopController.Intance.ListOfUnitStatusItem[0].QIndex.Objindex;
			currentTrainingTime = trainingTimes[objIndex];
			ShopController.Intance.ListOfUnitStatusItem[0].Slider.value += ((Time.deltaTime)/trainingTimes[objIndex]);
			currentSlidVal = ShopController.Intance.ListOfUnitStatusItem[0].Slider.value;			
			ShopController.Intance.ListOfUnitStatusItem[0].Slider.value = Mathf.Clamp(ShopController.Intance.ListOfUnitStatusItem[0].Slider.value,0,1);
			
			if(Math.Abs(ShopController.Intance.ListOfUnitStatusItem[0].Slider.value - 1) < 0.1f)
			{ 
				FinishObject(0);
			}

			progCounter = 0;
			UpdateTime();	
		}				
	}
	
	private void FinishObject(int index)
	{		
		int objIndex = ShopController.Intance.ListOfUnitStatusItem[index].QIndex.Objindex;

		UpdateExistingUnits(index);
		
		Stats.Instance.UpdateUnitsNo();
		UnBuild(objIndex,1);	
	}

	public void UpdateExistingUnits(int index)
	{
		ExistedUnit existedUnit = new ExistedUnit();
		existedUnit.id = ShopController.Intance.ListOfUnitStatusItem[index].ItemData.id;
		existedUnit.count = ShopController.Intance.ListOfUnitStatusItem[index].QIndex.Count;
		existedUnit.level = ShopController.Intance.ListOfUnitStatusItem[index].Level;

		int indx = Stats.Instance.ExistingUnits.FindIndex(x => x.id == existedUnit.id && x.level == existedUnit.level);
		if (indx != -1)
		{
			Stats.Instance.ExistingUnits[indx].count += existedUnit.count;
			Stats.Instance.ExistingUnits[indx].level = existedUnit.level;
		}
		else
		{
			Stats.Instance.ExistingUnits.Add(existedUnit);

			//Fernando temporal workaround
			UnitInfo _unitInfo = new UnitInfo();
			_unitInfo.id = existedUnit.id;
			_unitInfo.count = Stats.Instance.ExistingUnits.Find(x => x.id == existedUnit.id).count;
			_unitInfo.level = existedUnit.level;
            MenuUnit.Instance.UnitsInfo.Add(_unitInfo);

		}
	}

	private void IncreasePopulation()
	{
		for (int i = 0; i < ShopController.Intance.ListOfUnitStatusItem.Count; i++)
		{
			UpdateExistingUnits(i);
		}
	}

	public void FinishNow()
	{
		if (priceInCrystals <= Stats.Instance.crystals) 
		{
			Stats.Instance.crystals -= priceInCrystals;	
			Stats.Instance.UpdateUI();
			ShopController.Intance.UpdateHitText("Training complete.");
			IncreasePopulation();
			Stats.Instance.UpdateUnitsNo();
			EraseValues();
		} 

		else if(timeRemaining > 0)			
		{
			MessageController.Instance.DisplayMessage("Not enough crystals");
		}
	}
}
