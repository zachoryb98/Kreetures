using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public enum InventoryUIState { ItemSelection, PartySelection, Busy, MoveToForget }

public class InventoryUI : MonoBehaviour
{
	[SerializeField] GameObject itemList;
	[SerializeField] ItemSlotUI itemSlotUI;

	[SerializeField] TextMeshProUGUI categoryText;
	[SerializeField] Image itemIcon;
	[SerializeField] TextMeshProUGUI itemDescription;

	[SerializeField] Image upArrow;
	[SerializeField] Image downArrow;

	[SerializeField] PartyScreen partyScreen;
	public MoveSelectionUI moveSelectionUI;

	Action<ItemBase> onItemUsed;

	int selectedItem = 0;
	int selectedCategory = 0;

	AttackBase moveToLearn;

	InventoryUIState state;

	const int itemsInViewport = 8;

	List<ItemSlotUI> slotUIList;
	Inventory inventory;
	RectTransform itemListRect;

	private void Awake()
	{
		inventory = Inventory.GetInventory();
		itemListRect = itemList.GetComponent<RectTransform>();
	}

	private void Start()
	{
		UpdateItemList();

		inventory.OnUpdated += UpdateItemList;
	}

	void UpdateItemList()
	{
		// Clear all the existing items
		foreach (Transform child in itemList.transform)
			Destroy(child.gameObject);

		slotUIList = new List<ItemSlotUI>();
		foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
		{
			var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
			slotUIObj.SetData(itemSlot);

			slotUIList.Add(slotUIObj);
		}

		UpdateItemSelection();
	}



	public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
	{
		StartCoroutine(DelayedHandleUpdate(onBack, onItemUsed));
	}

	private IEnumerator DelayedHandleUpdate(Action onBack, Action<ItemBase> onItemUsed)
	{
		//Wait needed for UX
		yield return new WaitForSeconds(0.5f); // Adjust the delay as needed

		if (state == InventoryUIState.ItemSelection)
		{
			this.onItemUsed = onItemUsed;

			int prevSelection = selectedItem;
			int prevCategory = selectedCategory;

			if (Input.GetKeyDown(KeyCode.DownArrow))
				++selectedItem;
			else if (Input.GetKeyDown(KeyCode.UpArrow))
				--selectedItem;
			else if (Input.GetKeyDown(KeyCode.RightArrow))
				++selectedCategory;
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
				--selectedCategory;

			//Allows endless left and right scrolling
			if (selectedCategory > Inventory.ItemCategories.Count - 1)
			{
				selectedCategory = 0;
			}
			else if (selectedCategory < 0)
			{
				selectedCategory = Inventory.ItemCategories.Count - 1;
			}

			selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

			if (prevCategory != selectedCategory)
			{
				ResetSelection();
				categoryText.text = Inventory.ItemCategories[selectedCategory];
				UpdateItemList();
			}

			if (prevSelection != selectedItem)
				UpdateItemSelection();

			//Wait needed for UX
			yield return new WaitForSeconds(0.5f); // Adjust the delay as needed

			if (Input.GetKeyDown(KeyCode.Z))
			{
				StartCoroutine(ItemSelected());
			}

			else if (Input.GetKeyDown(KeyCode.X))
			{
				onBack?.Invoke();
			}
		}
		else if (state == InventoryUIState.PartySelection)
		{
			//Handle Party Selection
			Action onSelected = () =>
			{
				StartCoroutine(UseItem());
			};

			Action onBackPartyScreen = () =>
			{
				ClosePartyScreen();
			};

			partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
		}
		else if (state == InventoryUIState.MoveToForget)
		{
			Action<int> onMoveSelected = (int moveIndex) =>
			{
				StartCoroutine(OnMoveToForgetSelected(moveIndex));
			};

			moveSelectionUI.HandleMoveSelection(onMoveSelected);
		}
	}

	IEnumerator ItemSelected()
	{
		state = InventoryUIState.Busy;

		var item = inventory.GetItem(selectedItem, selectedCategory);

		if (GameManager.Instance.state == GameState.Battle)
		{
			// In Battle
			if (!item.CanUseInBattle)
			{
				yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle");
				state = InventoryUIState.ItemSelection;
				yield break;
			}
		}
		else
		{
			// Outside Battle
			if (!item.CanUseOutsideBattle)
			{
				yield return DialogManager.Instance.ShowDialogText($"This item cannot be used outside battle");
				state = InventoryUIState.ItemSelection;
				yield break;
			}
		}

		if (selectedCategory == (int)ItemCategory.CaptureDevices)
		{
			StartCoroutine(UseItem());
		}
		else
		{
			OpenPartyScreen();

			if (item is LearnableItem)
				partyScreen.ShowIfTmIsUsable(item as LearnableItem);
		}
	}

	IEnumerator UseItem()
	{
		state = InventoryUIState.Busy;

		yield return HandleLearnableItems();

		var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
		if (usedItem != null)
		{
			if (usedItem is RecoveryItem)
				yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.ItemName}");

			onItemUsed?.Invoke(usedItem);
		}
		else
		{
			if (selectedCategory == (int)ItemCategory.Items)
				yield return DialogManager.Instance.ShowDialogText($"It won't have any affect!");
		}

		ClosePartyScreen();
	}

	IEnumerator HandleLearnableItems()
	{
		var learnableItem = inventory.GetItem(selectedItem, selectedCategory) as LearnableItem;
		if (learnableItem == null)
			yield break;

		var kreeture = partyScreen.SelectedMember;

		if (kreeture.HasMove(learnableItem.Attack))
		{
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} already know {learnableItem.Attack.AttackName}");
			yield break;
		}

		if (!learnableItem.CanBeTaught(kreeture))
		{
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} can't learn {learnableItem.Attack.AttackName}");
			yield break;
		}

		if (kreeture.Attacks.Count < KreetureBase.MaxNumOfMoves)
		{
			kreeture.LearnMove(learnableItem.Attack);
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} learned {learnableItem.Attack.AttackName}");
		}
		else
		{
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} is trying to learn {learnableItem.Attack.AttackName}");
			yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {KreetureBase.MaxNumOfMoves} moves");
			yield return ChooseMoveToForget(kreeture, learnableItem.Attack);
			yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
		}
	}

	IEnumerator ChooseMoveToForget(Kreeture kreeture, AttackBase newAttack)
	{
		state = InventoryUIState.Busy;
		yield return DialogManager.Instance.ShowDialogText($"Choose a move you wan't to forget", true, false);
		moveSelectionUI.gameObject.SetActive(true);
		moveSelectionUI.SetMoveData(kreeture.Attacks.Select(x => x.Base).ToList(), newAttack);
		moveToLearn = newAttack;		

		state = InventoryUIState.MoveToForget;
	}

	void UpdateItemSelection()
	{
		var slots = inventory.GetSlotsByCategory(selectedCategory);

		selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

		for (int i = 0; i < slotUIList.Count; i++)
		{
			if (i == selectedItem)
				slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
			else
				slotUIList[i].NameText.color = Color.white;
		}

		if (slots.Count > 0)
		{
			var item = slots[selectedItem].Item;
			itemIcon.sprite = item.Icon;
			itemDescription.text = item.Description;
		}

		HandleScrolling();
	}

	void HandleScrolling()
	{
		if (slotUIList.Count <= itemsInViewport)
			return;

		float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
		itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

		bool showUpArrow = selectedItem > itemsInViewport / 2;
		upArrow.gameObject.SetActive(showUpArrow);

		bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
		downArrow.gameObject.SetActive(showDownArrow);
	}

	void ResetSelection()
	{
		selectedItem = 0;

		upArrow.gameObject.SetActive(false);
		downArrow.gameObject.SetActive(false);

		itemIcon.sprite = null;
		itemDescription.text = "";
	}

	void OpenPartyScreen()
	{
		state = InventoryUIState.PartySelection;
		partyScreen.gameObject.SetActive(true);
	}

	void ClosePartyScreen()
	{
		state = InventoryUIState.ItemSelection;

		partyScreen.ClearMemberSlotMessages();
		partyScreen.gameObject.SetActive(false);
	}

	IEnumerator OnMoveToForgetSelected(int moveIndex)
	{
		var kreeture = partyScreen.SelectedMember;

		DialogManager.Instance.CloseDialog();
		moveSelectionUI.gameObject.SetActive(false);
		if (moveIndex == KreetureBase.MaxNumOfMoves)
		{
			//Don't learn the new move
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} did not learn {moveToLearn.AttackName}");
		}
		else
		{
			//Forget the selected move and learn new move
			var selectedMove = kreeture.Attacks[moveIndex].Base;
			yield return DialogManager.Instance.ShowDialogText($"{kreeture.Base.Name} forgot {selectedMove.AttackName} and learned {moveToLearn.AttackName}");

			kreeture.Attacks[moveIndex] = new Attack(moveToLearn);
		}

		moveToLearn = null;
		state = InventoryUIState.ItemSelection;
	}
}
