/**
 * This source file is part of CityBuildingKit.
 * Copyright (c) 2018.
 * All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UIControllersAndData.Store;
using Assets.Scripts.UIControllersAndData.Store.Categories.Cloak;
using Assets.Scripts.UIControllersAndData.Store.Categories.Store;
using Assets.Scripts.UIControllersAndData.Store.ShopItems.Cloak;
using JetBrains.Annotations;
using UIControllersAndData.Models;
using UIControllersAndData.Store.Categories.Ambient;
using UIControllersAndData.Store.Categories.Buildings;
using UIControllersAndData.Store.Categories.Military;
using UIControllersAndData.Store.Categories.Unit;
using UIControllersAndData.Store.Categories.Walls;
using UIControllersAndData.Store.Categories.Weapon;
using UIControllersAndData.Store.ShopItems;
using UIControllersAndData.Store.ShopItems.Building;
using UIControllersAndData.Store.ShopItems.StoreItem;
using UIControllersAndData.Store.ShopItems.UnitItem;
using UnityEngine;
using UnityEngine.UI;

namespace UIControllersAndData.Store
{
    public class ShopController:MonoBehaviour
    {
        public static ShopController Intance;

        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private GridLayoutGroup _gridForUnits;
        [SerializeField] private Transform _parentForUnitStatusItem; 

        [SerializeField] private StoreItem _storeItem;
        [SerializeField] private BuildingItem _buildingItem;
        [SerializeField] private UnitItem _unitItem;
        [SerializeField] private CloakItem _cloakItem;
        [SerializeField] private UnitStatusItem _unitStatusItem;

        [SerializeField] private Button _closeButton;

        [SerializeField] private Text _timeToComplete;
        [SerializeField] private Text _finishButtonLabel;
        [SerializeField] private Text _hintText;
        
        [SerializeField] private StructureCreator _buildingCreator;
        [SerializeField] private StructureCreator _ambientCreator;
        [SerializeField] private StructureCreator _wallCreator;
        [SerializeField] private StructureCreator _weaponCreator;

        [SerializeField] private Button shopButton;
        [SerializeField] private Button unitsButton;

        private List<BaseShopItem> _listOfItemsInCategory = new List<BaseShopItem>();

        public List<BaseShopItem> ListOfItemsInCategory
        {
            get { return _listOfItemsInCategory; }
        }

        private List<UnitStatusItem> _listOfUnitStatusItem = new List<UnitStatusItem>();

        public List<UnitStatusItem> ListOfUnitStatusItem => _listOfUnitStatusItem;

        private void Awake()
        {
            Intance = this;
        }


        /// <summary>
        /// Clears a shop category
        /// </summary>
        /// <param name="shopCategoryType"></param>
        private void ClearShopCategory(ShopCategoryType shopCategoryType)
        {
            Transform content = shopCategoryType == ShopCategoryType.Unit ? _gridForUnits.transform : _grid.transform;
            
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void DrawCategory(IEnumerable<DrawCategoryData> items, ShopCategoryType shopCategoryType)
        {
            ClearShopCategory(shopCategoryType);    
            _listOfItemsInCategory.Clear();
            
            foreach (var data in items)
            {
                
                BaseShopItem cell;
                cell = CreateCell(shopCategoryType, data);
                if (cell != null && data.BaseItemData != null)
                {
                    AddListener(cell, shopCategoryType, data);
                    switch (shopCategoryType)
                    {
                        case ShopCategoryType.Store:
                            global::Store.Instance.InitUIStoreItems();
                            break;				
                        case ShopCategoryType.Buildings:
                        case ShopCategoryType.Military:
                            _buildingCreator.ConfigureQuantityForItem(data.Id.GetId());
                            break;
                        case ShopCategoryType.Walls:
                            _wallCreator.ConfigureQuantityForItem(data.Id.GetId());
                            break;
                        case ShopCategoryType.Weapon:
                            _weaponCreator.ConfigureQuantityForItem(data.Id.GetId());
                            break;
                        case ShopCategoryType.Ambient:
                            _ambientCreator.ConfigureQuantityForItem(data.Id.GetId());
                            break;	
                    }
                }
            }

            var count = items.Count();
            _grid.constraintCount = count > 8 ? Mathf.CeilToInt(count/2.0f) : 4;
        }

        private BaseShopItem CreateCell(ShopCategoryType shopCategoryType, DrawCategoryData data)
        {
            BaseShopItem cell = null;

            if(shopCategoryType == ShopCategoryType.Store && TransData.Instance.ListOfIdSoldItems.Contains(data.Id.GetId()))
            {
                return null;
            }

            switch (shopCategoryType)
            {
                case ShopCategoryType.Store:
                    cell = Instantiate(_storeItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;				
                case ShopCategoryType.Buildings:
                case ShopCategoryType.Military:
                    cell = Instantiate(_buildingItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;
                case ShopCategoryType.Walls:
                    cell = Instantiate(_buildingItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;
                case ShopCategoryType.Weapon:
                    cell = Instantiate(_buildingItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;
                case ShopCategoryType.Unit:
                    cell = Instantiate(_unitItem, _gridForUnits.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;
                case ShopCategoryType.Cloak:
                    cell = Instantiate(_cloakItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;
                case ShopCategoryType.Ambient:
                    cell = Instantiate(_buildingItem, _grid.transform);
                    cell.Initialize(data, shopCategoryType);
                    _listOfItemsInCategory.Add(cell);
                    break;	
            }
            return cell;
        }
        
        private void AddListener(BaseShopItem cell, ShopCategoryType shopCategoryType, DrawCategoryData itemData)
        {
            cell.OnClick += () =>
            {
                switch (shopCategoryType)
                {
                    case ShopCategoryType.Store:
                        global::Store.Instance.BuyStoreItem(itemData);
                        break;
                    case ShopCategoryType.Buildings:
                    case ShopCategoryType.Military:
                        _buildingCreator.BuyStoreItem(itemData, shopCategoryType,
                            () => { _closeButton.onClick.Invoke(); });
                        break;
                    case ShopCategoryType.Ambient:    
                        _ambientCreator.BuyStoreItem(itemData, shopCategoryType,
                            () => { _closeButton.onClick.Invoke(); });
                        break;
                    case ShopCategoryType.Weapon:    
                        _weaponCreator.BuyStoreItem(itemData, shopCategoryType,
                            () => { _closeButton.onClick.Invoke(); });
                        break;
                    case ShopCategoryType.Walls:    
                        _wallCreator.BuyStoreItem(itemData, shopCategoryType,
                            () => { _closeButton.onClick.Invoke(); });
                        break;
                    case ShopCategoryType.Unit:
                        MenuUnit.Instance.BuyStoreItem(itemData.BaseItemData as UnitCategory, 
                            () => { AddStatusUnit(itemData.BaseItemData as UnitCategory, 1); });
                        break;
                }
            };
        }
        
        [UsedImplicitly]
        public void OnStoreCategoryHandler(int categoryIndex)
        {
            OnOpenShop();
            IEnumerable<DrawCategoryData> drawCategoryData;
            object category;
            switch (categoryIndex)
            {
                case 0:
                    category = ShopData.Instance.StoreCategoryData;
                    drawCategoryData = ((StoreCategoryData) category).Category.Select(item =>
                        new DrawCategoryData {BaseItemData = item, Id = item, Name = item}); 
                    DrawCategory(drawCategoryData, ShopCategoryType.Store);
                    break;
                case 1:
                    category = ShopData.Instance.BuildingsCategoryData;
                    var buildingCategory = ((BuildingsCategoryData) category); 
                    var buildingsCategories = buildingCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(buildingsCategories, ShopCategoryType.Buildings);
                    break;
                case 2:
                    category = ShopData.Instance.MilitaryCategoryData;
                    var militaryCategory = ((MilitaryCategoryData) category); 
                    var militaryCategories = militaryCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(militaryCategories, ShopCategoryType.Military);
                    break;
                case 3:
                    category = ShopData.Instance.WallsCategoryData;
                    var wallCategory = ((WallsCategoryData) category);
                    var wallCategories = wallCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(wallCategories, ShopCategoryType.Walls);
                    break;
                case 4:
                    category = ShopData.Instance.WeaponCategoryData;
                    var weaponCategory = ((WeaponCategoryData) category); 
                    var weaponCategories = weaponCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(weaponCategories, ShopCategoryType.Weapon);
                    break;
                case 5:
                    category = ShopData.Instance.UnitCategoryData;
                    var unitCategory = ((UnitCategoryData) category); 
                    var unitCategories = unitCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(unitCategories, ShopCategoryType.Unit);
                    MenuUnit.Instance.LoadValuesfromProc();
                    break;
                case 6:
                    category = ShopData.Instance.CloakCategoryData;
                    drawCategoryData = ((CloakCategoryData) category).Category.Select(item =>
                        new DrawCategoryData {BaseItemData = item, Id = item, Name = item});
                    DrawCategory(drawCategoryData, ShopCategoryType.Cloak);
                    break;
                case 7:
                    category = ShopData.Instance.AmbientCategoryData;
                    var ambientCategory = ((AmbientCategoryData) category); 
                    var ambientCategories = ambientCategory.category.Select(item =>
                    { 
                        var baseData = item.levels.Find(level => level.level == 1);
                        return new DrawCategoryData{BaseItemData = baseData, Id = item, Name = item};
                    });
                    DrawCategory(ambientCategories, ShopCategoryType.Ambient);
                    break;
            }
        }

        [UsedImplicitly]
        public void OnOpenShop()
        {
            CameraController.Instance.enabled = false;
            MenuUnit.Instance.LoadValuesfromProc();
        }
        
        [UsedImplicitly]
        public void OnCloseShop()
        {
            CameraController.Instance.enabled = true;
            MenuUnit.Instance.PassValuestoProc();
        }

        [UsedImplicitly]
        public void OnFinishUnitBuilding()
        {
            MenuUnit.Instance.FinishNow();
        }

        public void AddStatusUnitFromSave(int unitId)
        {
            var levels = ShopData.Instance.GetLevels(unitId, ShopCategoryType.Unit);

            var unitData = levels?.FirstOrDefault(x => ((ILevel) x).GetLevel() == 1);
            if (unitData != null)
            {
                AddStatusUnit(unitData as UnitCategory, 1);
            }
        }

        private void AddStatusUnit(UnitCategory data, int level)
        {   
            var unit = _listOfUnitStatusItem.Find(x => x.ItemData.GetId() == data.GetId());
            if (!unit)
            {
                unit = Instantiate(_unitStatusItem, _parentForUnitStatusItem);
                _listOfUnitStatusItem.Add(unit);
                
            }
            unit.Initialize(data, level);
        }

        public void UpdateUnitStatusData(string time, string price)
        {
            _timeToComplete.text = time;
            _finishButtonLabel.text = price;
            
            //TODO: show ((UILabel)HintLabel).text ="Training canceled.";
        }

        public void UpdateHitText(string text)
        {
            _hintText.text = text;
        }

        public void UpdateUnitsCountAndProgess(int id, int count, float progress = 0)
        {
            var unit = _listOfUnitStatusItem.Find(x => x.ItemData.GetId() == id);
            if (unit)
            {
                unit.Count.text = count.ToString();
                unit.QIndex.Count = count;
                unit.Slider.value = progress;
                //TODO: update hint
            }
        }
        
        public void UpdateUnitProgess(int id, float progress)
        {
            var unit = _listOfUnitStatusItem.Find(x => x.ItemData.GetId() == id);
            if (unit)
            {
                unit.Slider.value = progress;
            }
        }

        public void RemoveStatusItemFromList(int index)
        {
            Destroy(_listOfUnitStatusItem[index].gameObject);
            _listOfUnitStatusItem.RemoveAt(index);
        }

        public void OpenUnitsCategory()
        {
            shopButton.onClick.Invoke();
            OnStoreCategoryHandler(5);
            unitsButton.onClick.Invoke();
        }
    }
}
