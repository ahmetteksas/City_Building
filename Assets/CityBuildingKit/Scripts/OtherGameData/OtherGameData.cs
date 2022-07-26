/**
 * This source file is part of CityBuildingKit.
 * Copyright (c) 2018.
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.OtherGameData;
using UnityEngine;

namespace UIControllersAndData.Store
{	
	public class OtherGameData : MonoBehaviour
	{
		private static OtherGameData _instance;

		public static OtherGameData Instance => _instance ?? (_instance = new OtherGameData());



		private RemovablesData _removables;

		public RemovablesData Removables
		{
			get
			{
				if (_removables == null)
				{
					_removables =
						(RemovablesData) LoadGameDataTable<RemovablesData>(OtherGameDataType.Removables);
				}
				return _removables;
			}
		}
		private GrassData _grass;

		public GrassData Grass
		{
			get
			{
				if (_grass == null)
				{
					_grass =
						(GrassData) LoadGameDataTable<GrassData>(OtherGameDataType.Grass);
				}
				return _grass;
			}
		}
		private ProgressStatusData _progressStatus;

		public ProgressStatusData ProgressStatus
		{
			get
			{
				if (_progressStatus == null)
				{
					_progressStatus =
						(ProgressStatusData) LoadGameDataTable<ProgressStatusData>(OtherGameDataType.ProgressStatusPrefabs);
				}
				return _progressStatus;
			}
		}
		
		private void Awake() {
			
					if (_instance != null && _instance != this)
					{
						Destroy(this.gameObject);
					} else {
						_instance = this;
					}
		}
		/// <summary>
		/// Loads shop category cap table
		/// </summary>
		/// <returns></returns>
		private static object LoadGameDataTable<T>(OtherGameDataType categoryType)
		{
			switch (categoryType)
			{
				case OtherGameDataType.Removables:
					return (T) (object) Resources.Load<RemovablesData>(Constants.PATH_FOR_REMOVABLES_CATEGORY_ASSET_LOAD);					
				case OtherGameDataType.Grass:
					return (T) (object) Resources.Load<GrassData>(Constants.PATH_FOR_GRASS_CATEGORY_ASSET_LOAD);					
				case OtherGameDataType.ProgressStatusPrefabs:
					return (T) (object) Resources.Load<ProgressStatusData>(Constants.PATH_FOR_PROGRESS_STATUS_PREFABS_CATEGORY_ASSET_LOAD);					
			}
			return null;
		}
		

	}
}