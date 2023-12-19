using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact()
    {
        if (!Used)
        {
            if(item != null)
            {
                var player = GameManager.Instance.playerController;
                player.GetComponent<Inventory>().AddItem(item);

                Used = true;

                // Get parent object and disable it
                Transform parentTransform = transform.parent;
                if (parentTransform != null)
                {
                    // Disable the parent object
                    parentTransform.gameObject.SetActive(false);
                }

                yield return DialogManager.Instance.ShowDialogText($"{player.name} found {item.name}");
            }            
        }
    }
}
