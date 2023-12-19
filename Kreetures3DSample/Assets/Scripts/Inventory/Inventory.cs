using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum ItemCategory { Items, CaptureDevices, Attacks}

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
	[SerializeField] List<ItemSlot> captureDeviceSlots;
	[SerializeField] List<ItemSlot> newMoveSlots;

    List<List<ItemSlot>> allSlots;

	public event Action OnUpdated;

	private void Awake()
	{
        allSlots = new List<List<ItemSlot>>() { slots, captureDeviceSlots, newMoveSlots };
	}

	public static List<String> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS", "CAPTURE DEVICES", "ATTACKS"
    };

    //Returns all slots
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex) => allSlots[categoryIndex];

	public ItemBase GetItem(int itemIndex, int categoryIndex)
	{
		var currentSlots = GetSlotsByCategory(categoryIndex);
		return currentSlots[itemIndex].Item;
	}

	public ItemBase UseItem(int itemIndex, Kreeture selectedKreeture, int selectedCategory)
	{
		var item = GetItem(itemIndex, selectedCategory);
		bool itemUsed = item.Use(selectedKreeture);
		if (itemUsed)
		{
			if (!item.IsReusable)
				RemoveItem(item, selectedCategory);

			return item;
		}

		return null;
	}

    public void AddItem(ItemBase item, int count=1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

	public void RemoveItem(ItemBase item, int category)
    {
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
			currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
            return ItemCategory.Items;
        else if (item is CaptureDeviceItem)
            return ItemCategory.CaptureDevices;
        else
            return ItemCategory.Attacks;
              
    }

    public static Inventory GetInventory()
    {
        return GameManager.Instance.playerController.GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}
