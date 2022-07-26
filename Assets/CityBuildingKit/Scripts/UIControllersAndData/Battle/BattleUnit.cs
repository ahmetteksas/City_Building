/**
 * This source file is part of CityBuildingKit.
 * Copyright (c) 2018.
 * All rights reserved.
 */
 
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit:MonoBehaviour 
{
	[SerializeField] private int _index;
    [SerializeField] private Text _count;
    [SerializeField] private Text _countOfSelected;

    public void Initialize(int count)
    {
        _count.text = count.ToString();
    }
    
    [UsedImplicitly]
	public void IncreaseUnit()
    {
		MenuArmyBattle.instance.Commit (_index);
    }
    
    [UsedImplicitly]
	public void DecreaseUnit()
    {
		MenuArmyBattle.instance.Cancel (_index);
    }

	[UsedImplicitly]
	public void ControlUnitCount(bool isOn)
	{
		MenuArmyBattle.instance.CommitAll (_index, isOn);
	}
}
