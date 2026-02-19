using System;
using SFS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SFS.UI
{
	public class ShopManager : MonoBehaviour
	{
		[Serializable]
		public class BuyButton
		{
			public Button button;
		}

		public GameObject openShopButton;

		public GameObject openShopButtonSale;

		public Text saleText_Home;

		public RectTransform saleText_BuyButton;

		[Space]
		public Screen_Menu shopMenu;

		public ProductThumbnail partsThumbnail;

		public ProductThumbnail redstoneAtlasPackThumbnail;

		public ProductThumbnail skinsThumbnail;

		public ProductThumbnail planetsThumbnail;

		public ProductThumbnail cheatsThumbnail;

		public ProductThumbnail infiniteAreaThumbnail;

		public ProductThumbnail builderBundleThumbnail;

		public ProductThumbnail cheatsBundleThumbnail;

		public ProductThumbnail fullBundleThumbnail;

		public ProductThumbnail newFullBundleThumbnail;

		public Button restoreButton;

		public BuyButton buyButton_Parts;

		public BuyButton buyButton_RedstoneAtlasPack;

		public BuyButton buyButton_Skins;

		public BuyButton buyButton_Planets;

		public BuyButton buyButton_Cheats;

		public BuyButton buyButton_InfiniteArea;

		public BuyButton buyButton_BuilderBundle;

		public BuyButton buyButton_CheatsBundle;

		public BuyButton buyButton_FullBundle;

		public BuyButton buyButton_NewFullBundle;

		public static bool showThanksMessage;

		public Text cheatsContent_1;

		public Text cheatsContent_2;

		public Text cheatsContent_3;

		public Text cheatsContent_4;

		public Text builderBundleContent;

		public Text sandboxBundleContent;

		public Text fullBundleContent;

		public Text newFullBundleContent;

		public Text partsTextB;

		public Text skinsTextB;

		public Text cheatsTextS;

		public Text infiniteAreaBundleS;

		public Text partsTextF;

		public Text skinsTextF;

		public Text planetsTextF;

		public Text cheatsTextF;

		public Text infiniteAreaBundleF;

		public Text partsTextNF;

		public Text redstoneAtlasPackNF;

		public Text skinsTextNF;

		public Text planetsTextNF;

		public Text cheatsTextNF;

		public Text infiniteAreaBundleNF;
	}
}
