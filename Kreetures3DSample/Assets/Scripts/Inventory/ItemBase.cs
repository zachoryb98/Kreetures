using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string itemName;
    [SerializeField] string description;
    [SerializeField] Sprite icon;

    public virtual string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;

    public virtual bool Use(Kreeture kreeture)
    {
        return false;
    }

	public virtual bool IsReusable => false;

	public virtual bool CanUseInBattle => true;
	public virtual bool CanUseOutsideBattle => true;
}
