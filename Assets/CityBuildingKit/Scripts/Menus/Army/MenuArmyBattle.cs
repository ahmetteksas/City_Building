using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Menus;
using Assets.Scripts.UIControllersAndData.Store;
using UIControllersAndData.Models;
using UIControllersAndData.Store;
using UIControllersAndData.Store.Categories.Unit;
using UnityEngine;
using UnityEngine.UI;

public class MenuArmyBattle : MonoBehaviour 
{
	public static MenuArmyBattle instance;

	public Camera mainCamera;//for some reason the SpriteLightKitCam is creating an input problem sometimes - its camera is recorded as Camera.main 

	private int unitsNo {
		
		get{
			return UIControllersAndData.Store.ShopData.Instance.UnitCategoryData.category.Count;
		}
	}
	
	private const int 
		unitGroupsNo = 4,
		zeroZ = 0;

	private bool deploying;

	private float 
		deployTimer,
		deployTime = 0.1f,
		speedModifier = 0.2f,
		touchTimer,			//prevent recording multiple spawn points with a single/hold mouse click
		touchTime = 0.1f;

	private int 
		spawnIndex,
		unitTypeIndex;

	public Text[] 
		availableUnitsNo ,				//battle = comes from existingUnitsNo in own map
		deployedUnitsNo ;			//deployed = comes from battleUnitsNo

	public Image[] existingUnitsPics ;

	public Button[] minusBt ;
	public Button[] deployAllButtons ;
	public Button[] returnAllButtons ;
	public GameObject[] UnitGroupBt = new GameObject[unitGroupsNo];

	public GameObject 
		spawnPointStar0, spawnPointStarI, spawnPointStarII, spawnPointStarIII,			//prefabs for the stars you click on the edge of the map
		deployBt, closeBt;																		//we need to deactivate this button at the end of the battle		

	private GameObject GroupUnits;

	private List<GameObject> 
		tempList = new List<GameObject> (),
		starList = new List<GameObject> ();

	private List<Vector3> spawnPointList = new List<Vector3> ();

	public Vector3 spawnPoint = new Vector3(0, 0, zeroZ);//cycles in the list and spreads the units on the map

	private Component helios, statsBattle, soundFx, relay;

	void Awake()
	{
		instance = this;

	}

	void Start () {

		GroupUnits = GameObject.Find("GroupUnits");
//
		helios = GameObject.Find ("Helios").GetComponent<Helios> ();
		statsBattle = GameObject.Find ("StatsBattle").GetComponent<StatsBattle> ();
//		heliosMsg = GameObject.Find ("HeliosMsg").GetComponent<MessageController> ();
		soundFx = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
		Debug.Log(unitsNo.ToString()+" different units");
		// existingUnitsPics = new Image[unitsNo];
		// availableUnitsNo = new Text[unitsNo];				//battle = comes from existingUnitsNo in own map
		// deployedUnitsNo = new Text[unitsNo];					//deployed = comes from battleUnitsNo
		// minusBt = new Button[unitsNo];
		// deployAllButtons = new Button[unitsNo];
		// returnAllButtons = new Button[unitsNo];		
//
//		UpdateMinusButtons();
		StartCoroutine (UpdateExistingUnits());
	}

	private void Delay()//to prevent button commands from interfering with sensitive areas/buttons underneath
	{
		((Relay)relay).DelayInput();
	}

	//this is called by 4 invisible tk2d buttons on the edge of the screen
	//Anchor/ center/ UnitsBattle/ deploypad

	private void RecordSpawnPoint() 
	{
		if (!((Helios)helios).networkLoadReady||((Relay)relay).delay)
						return;
		
		if (deploying)
		{
			MessageController.Instance.DisplayMessage("Deployment in progress. Please wait.");
			return;
		}

		Vector3 gridPos = new Vector3(0,0,0);

		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.back, new Vector3(0, 0, 0));//transform.position + 

		// Generate a ray from the cursor position

		Ray RayCast;

		if (Input.touchCount > 0)
			RayCast = mainCamera.ScreenPointToRay(Input.GetTouch(0).position);
		else
			RayCast = mainCamera.ScreenPointToRay(Input.mousePosition);// Camera.main.

		// Determine the point where the cursor ray intersects the plane.
		float HitDist = 0;
		
		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast(RayCast, out HitDist))//playerPlane.Raycast
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 RayHitPoint = RayCast.GetPoint(HitDist);
						
			int indexCell = GridManager.instance.GetGridIndex(RayHitPoint);
			
			int col = GridManager.instance.GetColumn(indexCell);
			int row = GridManager.instance.GetRow(indexCell);
				
			if(row>0 && row<34 && col> 0 && col<34)//force click inside map
			{
				if(!GridManager.instance.nodes[row,col].isObstacle)
			{
				gridPos = GridManager.instance.nodes[row,col].position;
				CreateStar (gridPos);
			}
			}		
		}
	}

	private void CreateStar(Vector3 gridPos)
	{
		((SoundFX)soundFx).SoldierPlace();
		int currentGroup = ((Helios)helios).instantiationGroupIndex;

		Vector3 starPos = gridPos + new Vector3 (0, 0, -3);

		switch (currentGroup) 
		{
		case -1:
			GameObject StarO = Instantiate (spawnPointStar0, starPos, Quaternion.identity);
			break;
		case 0:
			GameObject StarI = Instantiate (spawnPointStarI, starPos, Quaternion.identity);	
			break;
		case 1:
			GameObject StarII = Instantiate (spawnPointStarII, starPos, Quaternion.identity);	
			break;
		case 2:
			GameObject StarIII = Instantiate (spawnPointStarIII, starPos, Quaternion.identity);	
			break;	
		case 3:
			MessageController.Instance.DisplayMessage("You already deployed all 4 squads.");
		break;
		}

		//Vector3 pos = new Vector3 (gridPos.x, gridPos.y, -3);
		spawnPointList.Add(gridPos);

		GameObject[] stars = GameObject.FindGameObjectsWithTag ("Star");
		
		for (int i = 0; i < stars.Length; i++) 		
		{
			if(stars[i].GetComponent<Selector>().isSelected)
			{
				starList.Add(stars[i]);
				stars[i].GetComponent<Selector>().isSelected = false;
				break;
			}
		}
	}

	private void DestroyStars()
	{
		for (int i = 0; i < starList.Count; i++) 
		{
			((Star)starList[i].GetComponent("Star")).die = true;
		}
		starList.Clear ();
		spawnPointList.Clear ();
	}

	public void Commit0()	{Commit (0);}
	public void Commit1()	{Commit (1);}
	public void Commit2()	{Commit (2);}
	public void Commit3()	{Commit (3);}
	public void Commit4()	{Commit (4);}
	public void Commit5()	{Commit (5);}
	public void Commit6()	{Commit (6);}
	public void Commit7()	{Commit (7);}
	public void Commit8()	{Commit (8);}
	public void Commit9()	{Commit (9);}
	


	public void Cancel0(){Cancel (0);}
	public void Cancel1(){Cancel (1);}
	public void Cancel2(){Cancel (2);}
	public void Cancel3(){Cancel (3);}
	public void Cancel4(){Cancel (4);}
	public void Cancel5(){Cancel (5);}
	public void Cancel6(){Cancel (6);}
	public void Cancel7(){Cancel (7);}
	public void Cancel8(){Cancel (8);}
	public void Cancel9(){Cancel (9);}
	

		
	public void All0()	{CommitAll (0, true);}
	public void All1()	{CommitAll (1, true);}
	public void All2()	{CommitAll (2, true);}
	public void All3()	{CommitAll (3, true);}
	public void All4()	{CommitAll (4, true);}
	public void All5()	{CommitAll (5, true);}
	public void All6()	{CommitAll (6, true);}
	public void All7()	{CommitAll (7, true);}
	public void All8()	{CommitAll (8, true);}
	public void All9()	{CommitAll (9, true);}
	

	
	public void None0()	{CommitAll (0, false);}
	public void None1()	{CommitAll (1, false);}
	public void None2()	{CommitAll (2, false);}
	public void None3()	{CommitAll (3, false);}
	public void None4()	{CommitAll (4, false);}
	public void None5()	{CommitAll (5, false);}
	public void None6()	{CommitAll (6, false);}
	public void None7()	{CommitAll (7, false);}
	public void None8()	{CommitAll (8, false);}
	public void None9()	{CommitAll (9, false);}
	


	public void ActivateDeployBt()
	{
		if(!deployBt.activeSelf)
			deployBt.SetActive(true);
	}

	public void DeactivateDeployBt()
	{
		if(deployBt.activeSelf)
		deployBt.SetActive(false);
	}

	private void UpdateMinusButtons()
	{
		for (int i = 0; i < unitsNo; i++) 
		{
			if (((StatsBattle)statsBattle).DeployedUnits[i].count > 0) 
			{
				minusBt[i].gameObject.SetActive(true);
			}
			else
				minusBt[i].gameObject.SetActive(false);

			if (((StatsBattle)statsBattle).AvailableUnits[i].count > 0) 
			{
				CommitAllButtons(i, true);
			}
		}
	}

	private void CommitAllButtons(int i, bool b)
	{
		deployAllButtons [i].gameObject.SetActive(b); // = b;
		returnAllButtons[i].gameObject.SetActive(!b);
	}
	public void CommitAll(int i, bool all)
	{
		if (deploying)
		{
			MessageController.Instance.DisplayMessage("Deployment in progress. Please wait.");
			return;
		}
		
		Delay ();//brief delay to prevent stars from appearing under the menus

		if(all)
		{
			if (((StatsBattle)statsBattle).AvailableUnits[i].count > 0) 
			{
				if (((StatsBattle)statsBattle).DeployedUnits[i].count == 0)
				{
					minusBt[i].gameObject.SetActive(true);
				}

				((StatsBattle)statsBattle).DeployedUnits[i].count += ((StatsBattle)statsBattle).AvailableUnits[i].count;
				((StatsBattle)statsBattle).DeployedUnits[i].id = ((StatsBattle)statsBattle).AvailableUnits[i].id;
				((StatsBattle)statsBattle).DeployedUnits[i].level = ((StatsBattle)statsBattle).AvailableUnits[i].level;
				((StatsBattle)statsBattle).AvailableUnits[i].count = 0;

				CommitAllButtons(i, false);

			} 
			else 
			{
				return;
			}
		}
		else 
		{
			if (((StatsBattle)statsBattle).DeployedUnits[i].count > 0) 
			{				
				((StatsBattle)statsBattle).AvailableUnits[i].count += ((StatsBattle)statsBattle).DeployedUnits[i].count;
				((StatsBattle)statsBattle).DeployedUnits[i].count = 0;

				minusBt[i].gameObject.SetActive(false);
				CommitAllButtons(i, true);
			} 
			else 
			{
				return;
			}
		}

		UpdateUnits ();
	}

	public void Commit(int i)
	{
		if (deploying)
		{
			MessageController.Instance.DisplayMessage("Deployment in progress. Please wait.");
			return;
		}

		Delay ();//brief delay to prevent stars from appearing under the menus
		if (((StatsBattle)statsBattle).AvailableUnits[i].count > 0) 
		{
			if (((StatsBattle)statsBattle).DeployedUnits[i].count == 0) 
			{
				minusBt[i].gameObject.SetActive(true);
			}

			((StatsBattle)statsBattle).AvailableUnits[i].count --; 
			((StatsBattle)statsBattle).DeployedUnits[i].count ++;
			((StatsBattle)statsBattle).DeployedUnits[i].id = ((StatsBattle)statsBattle).AvailableUnits[i].id;
			((StatsBattle)statsBattle).DeployedUnits[i].level = ((StatsBattle)statsBattle).AvailableUnits[i].level;

			if (((StatsBattle)statsBattle).AvailableUnits[i].count == 0) 
			{
				CommitAllButtons(i, false);
			}
		} 
		else 
		{
			return;
		}

		UpdateUnits ();
	}

	public void Cancel(int i)
	{
		if (deploying)
		{
			MessageController.Instance.DisplayMessage("Deployment in progress. Please wait.");
			return;
		}
		Delay ();//brief delay to prevent stars from appearing under the menus
		if (((StatsBattle)statsBattle).DeployedUnits[i].count > 0) 
		{
			if (((StatsBattle)statsBattle).DeployedUnits[i].count == 1) 
			{
				minusBt[i].gameObject.SetActive(false);
			}
			((StatsBattle)statsBattle).DeployedUnits[i].count --;
			((StatsBattle)statsBattle).AvailableUnits[i].count++;
		} 
		else 
		{
			return;
		}

		UpdateUnits ();
	}

	public void UpdateStats()
	{
		StartCoroutine (UpdateExistingUnits());//cannot send command directly, takes a while to update from stats
	}

	private IEnumerator UpdateExistingUnits()//building reselect
	{
		yield return new WaitForSeconds(0.25f);
		UpdateUnits ();
	}

	private void UpdateUnits()
	{
		for (int i = 0; i < availableUnitsNo.Length; i++) 
		{
			if(((StatsBattle)statsBattle).AvailableUnits[i].count > 0 )
			{
				availableUnitsNo[i].text =  ((StatsBattle)statsBattle).AvailableUnits[i].count.ToString();
				existingUnitsPics[i].color = new Color(1,1,1);
			}

			else
			{
				availableUnitsNo[i].text = " ";
			}

			if(((StatsBattle)statsBattle).DeployedUnits[i].count > 0)
			{
				deployedUnitsNo[i].text = ((StatsBattle)statsBattle).DeployedUnits[i].count.ToString();
				existingUnitsPics[i].color = new Color(1,1,1);
			}
			else
			{
				deployedUnitsNo[i].text = " ";
			}

			if(((StatsBattle)statsBattle).AvailableUnits[i].count == 0 && ((StatsBattle)statsBattle).DeployedUnits[i].count == 0)
			{
				existingUnitsPics[i].color = new Color(0,0,0);
			}
		}
		UpdateMinusButtons ();
	}

	void Update()
	{
		if(deploying)
		{
			deployTimer+=Time.deltaTime;

			if(deployTimer>deployTime)
			{
				DeployInSequence();
				deployTimer = 0;
			}
		}



		touchTimer += Time.deltaTime;

		if (touchTimer > touchTime) 
		{
			touchTimer = 0;
			if (Input.GetMouseButton (0)) 
			{
				if(Input.mousePosition.y>130)
				RecordSpawnPoint ();
			}
			else if (Input.touchCount>0) 
			{
				if(Input.GetTouch (0).position.y>130)
					RecordSpawnPoint ();
			}
		}
	}

	private void DeployInSequence()
	{
		for (int i = unitTypeIndex; i < unitsNo; i++) //unitTypeIndex
		{
			int index = ((StatsBattle)statsBattle).DeployedUnits[i].count;
			if(index>0)
			{
				unitTypeIndex = i;									//to avoid starting from 0 each time
				spawnPoint = spawnPointList[spawnIndex];

				if(spawnIndex<spawnPointList.Count-1)				//distributes the units to all spawn points
					spawnIndex++;
				else
					spawnIndex=0;
				
				InstantiateUnit(i, speedModifier);					
				speedModifier += 0.2f;

				((StatsBattle)statsBattle).DeployedUnits[i].count --;		//units are deployed one by one
				UpdateInterface();	
				break;
			}

			if(i == unitsNo-1)								//finished deploying
			{

				UpdateInterface();							
				((StatsBattle)statsBattle).UpdateUnitsNo ();//update group units	

				int deployedIndex = ((Helios)helios).instantiationGroupIndex;	//the index we are deploying now
				((Helios)helios).ProcessUnitGroup(deployedIndex);

				DestroyStars ();
				deploying = false;
				((Relay)relay).deploying = false;
				closeBt.SetActive (true);
				break;
			}

		}
	}

	public void DeployUnits()
	{
		if (deploying)
		{
			MessageController.Instance.DisplayMessage("Deployment in progress. Please wait.");
			return;
		}

		Delay ();//brief delay to prevent stars from appearing under the menus or select building target at the same time
		if (starList.Count == 0)
		{
			MessageController.Instance.DisplayMessage("Select the location on the edge of the map.");
			return;//insert message
		}
			if (((Helios)helios).instantiationGroupIndex >= 3)	//already deployed all 4 groups
		{
			MessageController.Instance.DisplayMessage("You already deployed all 4 squads.");
			return;
		}
			if(!((Helios)helios).networkLoadReady)						//map not loaded yet - don't deploy
		{
			MessageController.Instance.DisplayMessage("Map is not loaded or internet connection failed.");
			return;//insert messages
		}

		bool someUnitSelected = false;

		for (int i = 0; i < unitsNo; i++) 
		{
			if(((StatsBattle)statsBattle).DeployedUnits[i].count != 0)
			{
				someUnitSelected = true;
				break;
			}
		}

		if (!someUnitSelected) //user has not selected any unit to deploy
		{
			MessageController.Instance.DisplayMessage("Assign units to the squad.");
			return;										
		}
				
		((Helios)helios).instantiationGroupIndex++;
		
		if(((Helios)helios).instantiationGroupIndex!=0)
			((Helios)helios).selectedGroupIndex++;

		((Relay)relay).deploying = true;

		switch (((Helios)helios).instantiationGroupIndex) 			//manages the last deployed group
		{
		case 0:
			tempList=((Helios)helios).GroupO;
			((Helios)helios).Select0();
			UnitGroupBt[0].SetActive(true);
			break;
		case 1:
			tempList=((Helios)helios).GroupI;
			((Helios)helios).Select1();
			UnitGroupBt[1].SetActive(true);
			break;
		case 2:
			tempList=((Helios)helios).GroupII;
			((Helios)helios).Select2();
			UnitGroupBt[2].SetActive(true);
			break;
		case 3:
			tempList=((Helios)helios).GroupIII;
			((Helios)helios).Select3();
			UnitGroupBt[3].SetActive(true);
			break;
		}		
		
		closeBt.SetActive (false);
		spawnIndex = 0;
		unitTypeIndex = 0;
		speedModifier = 0.2f;		//puts some distance between units while walking
		deploying = true;

	}

	private void InstantiateUnit(int index, float speedModifier)
	{
		int level = MenuArmy.Instance.ExistingBattleUnits.Find(x => x.id == index).level;
		
//		GameObject asset = ShopData.Instance.GetAssetForLevel(index, ShopCategoryType.Unit, level);

		var levels = ShopData.Instance.GetLevels(index, ShopCategoryType.Unit);
		if (levels == null)
		{
			return;
		}
		var unit = levels?.FirstOrDefault(x => ((ILevel) x).GetLevel() == level);
		if (unit == null)
		{
			return;
		}

		if (((UnitCategory) unit).asset)
		{
			Instantiate (((UnitCategory)unit).asset, spawnPoint, Quaternion.identity);
			ProcessUnit(speedModifier, unit);	
		}
		else
		{
			Debug.LogError("Asset is null: " + ((UnitCategory)unit).name);
		}
	}

	private void ProcessUnit(float speedModifier, BaseStoreItemData unit)
	{
		string unitType = "Unit";
		//remove this to process different units 

		GameObject[] units = GameObject.FindGameObjectsWithTag(unitType);				
		for (int i = 0; i < units.Length; i++) 
		{
			if(((Selector)units[i].GetComponent("Selector")).isSelected)
			{
				units[i].transform.parent = GroupUnits.transform;
				var fightController = units[i].GetComponent<FighterController>(); 
				fightController.speed += speedModifier;
				fightController.assignedToGroup = ((Helios)helios).selectedGroupIndex;
				if (unit != null)
				{
					fightController.Life = unit.HP;
					fightController.Shooter.fireRate = ((UnitCategory) unit).fireRate;
					fightController.DamagePoints =  ((UnitCategory) unit).damagePoints;
				}
				 
				
				tempList.Add(units[i]);
				((Helios)helios).DeployedUnits.Add(units[i]);
				((Helios)helios).instantiationUnitIndex++;
				units[i].GetComponent<Selector>().index = ((Helios)helios).instantiationUnitIndex;
				((Selector)units[i].GetComponent("Selector")).isSelected = false;
				break;
			}
		}
	}

	private void UpdateInterface()
	{
		/*
		for (int i = 0; i < unitsNo; i++) 
		{
			((StatsBattle)statsBattle).deployedUnits [i] = 0;
		}
		*/
		UpdateUnits();
	}
}
