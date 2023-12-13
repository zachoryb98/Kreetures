using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    

    public ItemBase UseItem(int itemIndex, Kreeture selectedKreeture, int selectedCategory)
    {
        var currentSlots = GetSlotsByCategory(selectedCategory);

        var item = currentSlots[itemIndex].Item;
        bool itemUsed = item.Use(selectedKreeture);
        if (itemUsed)
        {
            RemoveItem(item, selectedCategory);
            return item;
        }

        return null;
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

    public ItemBase Item => item;
    public int Count
    {
        get => count;
        set => count = value;
    }
}
