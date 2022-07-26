/**
 * This source file is part of CityBuildingKit.
 * Copyright (c) 2018.
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.OtherGameData
{
	[Serializable]
	public class RemovablesData:ScriptableObject 
	{
		public string RemovablesCategoryId;
		[FormerlySerializedAs("RemovablesCategory")]
		public List<GameObject> Category;
		public GameObject RemovableTimerPrefab;
		
	}
}