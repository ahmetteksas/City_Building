using UnityEngine;
using System.Collections;
using System;
[Serializable]
public class GameUnits_Settings : ScriptableObject
{

    [Header("Amount of units")]
    
	    public int buildingTypesNo ;			//the building tags and prefabs
	    public int resourcesBuildingTypesNo;			//the building tags and prefabs
	    public int militaryBuildingTypesNo;			//the building tags and prefabs
		public int grassTypesNo;	
		public int removableGrassTypesNo;	//single patch grass for removables tags and prefabs; they are auto random generated at load but not saved
		public int removableTypesNo;		//t;e removable tags and prefabs
		public int wallTypesNo ;
		public int fenceTypesNo ;
		public int weaponTypesNo;
		public int ambientTypesNo;
		public int unitsNo ; // number of units from Unit Category (Shop)
        public int maxUnitsNo ; //maximum number of units of each type that can be trained	

    [Header("Campaigns")]
        public int campaignNo ; 
}
