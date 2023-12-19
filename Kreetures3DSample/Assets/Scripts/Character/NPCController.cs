using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector3> movementPattern;
    [SerializeField] float timeBetweenPattern;
    NPCState state;

    public IEnumerator Interact()
    {
        if(itemGiver != null && itemGiver.CanBeGiven())
        {
            yield return itemGiver.GiveItem(GameManager.Instance.playerController);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
        }        

        state = NPCState.Idle;
    }

    private PlayerInput playerControls;
    private bool playerInRange = false;

    ItemGiver itemGiver;

    private void Awake()
    {
        playerControls = new PlayerInput();
        itemGiver = GetComponent<ItemGiver>();
    }

    private void OnEnable()
    {
        playerControls.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerControls.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}

public enum NPCState { Idle, Walking }
