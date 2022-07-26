using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UIControllersAndData.Store;
using UIControllersAndData.GameResources;
using UIControllersAndData.Models;
using UIControllersAndData.Store;
using UIControllersAndData.Store.Categories.Buildings;
using UIControllersAndData.Store.Categories.Walls;
using UIControllersAndData.Store.Categories.Military;
using UIControllersAndData.Store.Categories.Ambient;
using UIControllersAndData.Store.Categories.Weapon;
using UnityEngine;

public class StructureCreator : BaseCreator
{

	private int index;	//instead of xml index building 2,3 producing/storing something, we will have 0,1,2...
							//xml order: Forge Generator Vault Barrel Summon

	void Start () {

		InitializeComponents ();		//this is the only class who will initiate component, since there is no need to receive thee same call from all children
		ReadStructures();
		StartCoroutine("UpdateLabelStats");

        switch (structureXMLTag)
        {
            case "Building":
                {

                    List<BuildingCategoryLevels> buildingCategoryLevels = ShopData.Instance.BuildingsCategoryData.category;
                    List<BuildingsCategory> buildingsCategoryData = buildingCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == _structuressWithLevel).ToList();

                    List<MilitaryCategoryLevels> militaryCategoryLevels = ShopData.Instance.MilitaryCategoryData.category;
                    List<MilitaryCategory> militaryCategoryData = militaryCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == _structuressWithLevel).ToList();

                    totalStructures = buildingsCategoryData.Count + militaryCategoryData.Count;
                    structurePf = new GameObject[totalStructures];
					constructionTypes = new int[totalStructures];
					grassTypes = new int[totalStructures];
					pivotCorrections = new int[totalStructures];

                    for (int i = 0; i < buildingsCategoryData.Count; i++)
                    {
                        structurePf[i] = buildingsCategoryData[i].asset;
						constructionTypes[i] = buildingsCategoryData[i].constructionType;
						grassTypes[i] = buildingsCategoryData[i].grassType;
						pivotCorrections[i] = buildingsCategoryData[i].pivotCorrection;
                    }
                    int j = 0;
                    for (int i = buildingsCategoryData.Count; i < totalStructures; i++)
                    {
                        structurePf[i] = militaryCategoryData[j].asset;
						constructionTypes[i] = militaryCategoryData[j].constructionType;
						grassTypes[i] = militaryCategoryData[j].grassType;
						pivotCorrections[i] = militaryCategoryData[j].pivotCorrection;						
                        j++;
                    }

                    RegisterBasicEconomyValues(buildingsCategoryData);
                    RegisterBasicEconomyValues(militaryCategoryData);
                    break;
                }

            case "Wall":
                {

                    List<WallCategoryLevels> wallsCategoryLevels = ShopData.Instance.WallsCategoryData.category;
                    List<WallsCategory> wallsCategoryData = wallsCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == _structuressWithLevel).ToList();

                    totalStructures = wallsCategoryData.Count;
                    structurePf = new GameObject[totalStructures];
					constructionTypes = new int[totalStructures];
					grassTypes = new int[totalStructures];
					pivotCorrections = new int[totalStructures];					
					

                    for (int i = 0; i < wallsCategoryData.Count; i++)
                    {
                        structurePf[i] = wallsCategoryData[i].asset;
						constructionTypes[i] = wallsCategoryData[i].constructionType;
						grassTypes[i] = wallsCategoryData[i].grassType;
						pivotCorrections[i] = wallsCategoryData[i].pivotCorrection;						
                    }

                    break;
                }

            case "Weapon":
                {

                    List<WeaponCategoryLevels> weaponCategoryLevels = ShopData.Instance.WeaponCategoryData.category;
                    List<WeaponCategory> weaponCategoryData = weaponCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == _structuressWithLevel).ToList();

                    totalStructures = weaponCategoryData.Count;
                    structurePf = new GameObject[totalStructures];
					constructionTypes = new int[totalStructures];
					grassTypes = new int[totalStructures];
					pivotCorrections = new int[totalStructures];					
										

                    for (int i = 0; i < weaponCategoryData.Count; i++)
                    {
                        structurePf[i] = weaponCategoryData[i].asset;
						constructionTypes[i] = weaponCategoryData[i].constructionType;
						grassTypes[i] = weaponCategoryData[i].grassType;
						pivotCorrections[i] = weaponCategoryData[i].pivotCorrection;							
                    }

                    break;
                }

            case "Ambient":
                {

                    List<AmbientCategoryLevels> ambientCategoryLevels = ShopData.Instance.AmbientCategoryData.category;
                    List<AmbientCategory> ambientCategoryData = ambientCategoryLevels.SelectMany(level => level.levels).Where(c => c.level == _structuressWithLevel).ToList();

                    totalStructures = ambientCategoryData.Count;
                    structurePf = new GameObject[totalStructures];
					constructionTypes = new int[totalStructures];
					grassTypes = new int[totalStructures];
					pivotCorrections = new int[totalStructures];							

                    for (int i = 0; i < ambientCategoryData.Count; i++)
                    {
                        structurePf[i] = ambientCategoryData[i].asset;
						constructionTypes[i] = ambientCategoryData[i].constructionType;
						grassTypes[i] = ambientCategoryData[i].grassType;
						pivotCorrections[i] = ambientCategoryData[i].pivotCorrection;						
                    }

                    break;
                }

        }

    }
	
	private void RegisterBasicEconomyValues<T>(List<T> data) where T:IProdBuilding, IStoreBuilding, IStructure 
	{
		for (int i = 0; i < data.Count; i++) 
		{
			bool isvalid = false;

			if (data[i].GetProdType() != GameResourceType.None)
			{
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].ProdType = data[i].GetProdType();
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].ProdPerHour = data[i].GetProdPerHour();
				isvalid = true;
			}
			if (data[i].GetStoreType() != StoreType.None) 
			{
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].StoreType = data[i].GetStoreType().ToString();//Internal, Distributed
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].StoreResource = data[i].GetStoreResource().ToString();
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].StoreCap = data[i].GetStoreCap();
				isvalid = true;
			}

			if (isvalid) 
			{				
				((ResourceGenerator)resourceGenerator).basicEconomyValues [index].StructureType = data[i].GetStructureType();
				index++;
			}
		}
	}
}
