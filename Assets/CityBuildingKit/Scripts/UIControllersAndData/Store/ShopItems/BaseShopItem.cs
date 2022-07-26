/**
 * This source file is part of CityBuildingKit.
 * Copyright (c) 2018.
 * All rights reserved.
 */

using System;
using Assets.Scripts;
using Assets.Scripts.UIControllersAndData.Images;
using Assets.Scripts.UIControllersAndData.Store;
using JetBrains.Annotations;
using UIControllersAndData.Models;
using UnityEngine;
using UnityEngine.UI;

namespace UIControllersAndData.Store.ShopItems
{
	public class BaseShopItem:MonoBehaviour, IBaseShopItem
	{
		public Action OnClick;
		
		[SerializeField] private Image _backgroundImage;
		
		[SerializeField] protected Image BigImage;

		[SerializeField] private Image _smallIcon;
		[SerializeField] protected Text TitleText;
		[SerializeField] protected Text TimeLabel;
		
		[SerializeField] private Image _iconOnBuyButton;
		[SerializeField] protected Text BuyButtonLabel;

		public Text BuyButtonLabel1
		{
			get { return BuyButtonLabel; }
			set { BuyButtonLabel = value; }
		}

		[SerializeField] protected Text QuantityOfItem;

		public Text QuantityOfItem1
		{
			get => QuantityOfItem;
			set => QuantityOfItem = value;
		}

		public BaseStoreItemData ItemData { get; set; }

		[UsedImplicitly]
		public void OnItemClick()
		{
			if (OnClick != null)
			{
				OnClick();
			}
		}
		
		[UsedImplicitly]
		public void OnDescriptionClick()
		{
			if (ItemData is INamed namedItem)
			{
				DescriptionPanel.Instance.SetDescriptionInfo(namedItem.GetName(), ItemData.Description);
			}
			else
			{
				throw new Exception("named item is not null");
			}
		}

		public virtual void Initialize(DrawCategoryData data, ShopCategoryType shopCategoryType)
		{
			ItemData = data.BaseItemData;
			Id = data.Id.GetId();
			
			
			if (ItemData == null)
			{
				throw new Exception("Item data is null");
			}

			TitleText.text = data.Name.GetName();
			

			BuyButtonLabel.text = ItemData.Price.ToString();

			if (BigImage)
			{
				BigImage.enabled = !string.IsNullOrEmpty(ItemData.IdOfBigIcon);
				BigImage.sprite = ImageControler.GetImage(ItemData.IdOfBigIcon);	
			}

			if (_smallIcon)
			{
				_smallIcon.enabled = !string.IsNullOrEmpty(ItemData.IdOfSmallIcon);
				_smallIcon.sprite = ImageControler.GetImage(ItemData.IdOfSmallIcon);	
			}

			if (_iconOnBuyButton)
			{
				_iconOnBuyButton.enabled = !string.IsNullOrEmpty(ItemData.IdOfIconOnBuyButton);
				_iconOnBuyButton.sprite = ImageControler.GetImage(ItemData.IdOfIconOnBuyButton);
			}
		}

		public int Id { get; set; }

		public void UpdateQuantity(int quantity)
		{
			if (quantity == ItemData.MaxCountOfThisItem)
			{
				_backgroundImage.sprite = ImageControler.GetImage(Constants.ID_ITEM_BACKGROUND);
				BigImage.sprite = ImageControler.GetImage(ItemData.IdOfBlackBigIcon);
				BuyButtonLabel.transform.parent.GetComponent<Button>().interactable = false;
				TitleText.color = Color.black;
				TimeLabel.color = Color.black;
				QuantityOfItem.color = Color.black;
			}
			
			
			
			QuantityOfItem.text = quantity + "/" + ItemData.MaxCountOfThisItem;
		}
	}
}
