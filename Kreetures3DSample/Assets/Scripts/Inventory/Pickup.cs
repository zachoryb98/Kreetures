using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable, ISavable
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
                    parentTransform.GetComponent<MeshRenderer>().enabled = false;
                    parentTransform.GetComponent<SphereCollider>().enabled = false;
                    this.gameObject.GetComponent<BoxCollider>().enabled = false;
                }

                yield return DialogManager.Instance.ShowDialogText($"{player.name} found {item.name}");
            }            
        }
    }

    public object CaptureState()
    {
        return Used;
    }


    public void RestoreState(object state)
    {
        Used = (bool)state;

        if (Used)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
