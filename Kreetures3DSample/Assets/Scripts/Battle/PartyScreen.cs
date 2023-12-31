using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;
using static UnityEditor.Progress;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Kreeture> kreetures;    

    int selection = 0;
    public Kreeture SelectedMember => kreetures[selection];

	[SerializeField] InputActionAsset inputActions;

	//Party screen can be valled from different states like ActionSelection, runningTurn, and AboutToUse
	public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);        
        SetPartyData();

        GameManager.Instance.playerController.GetComponent<KreetureParty>().OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
		kreetures = GameManager.Instance.playerController.GetComponent<KreetureParty>().Kreetures;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < kreetures.Count)
			{                
                kreetures[i].Init();
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(kreetures[i]);
            }                
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Kreeture";
    }

	public void HandleUpdate(Action onSelected, Action onback)
	{               
        inputActions.Enable();
        var moveLeftAction = inputActions["MoveLeft"];
		var moveRightAction = inputActions["MoveRight"];
		var moveUpAction = inputActions["MoveUp"];
		var moveDownAction = inputActions["MoveDown"];
		var confirmAction = inputActions["PartyConfirm"];
		var backAction = inputActions["PartyBack"];

        var prevSelection = selection;

		if (moveRightAction.triggered)
			++selection;
		else if (moveLeftAction.triggered)
			--selection;
		else if (moveDownAction.triggered)
			selection += 2;
		else if (moveUpAction.triggered)
			selection -= 2;

		selection = Mathf.Clamp(selection, 0, kreetures.Count - 1);

        if (selection != prevSelection)
		{
            UpdateMemberSelection(selection);
        }		

		if (confirmAction.triggered)
		{
            onSelected?.Invoke();
		}
		else if (backAction.triggered)
		{            
            selection = 0;
			onback?.Invoke();
		}
	}

	public void UpdateMemberSelection(int selectedMember)
	{
		for (int i = 0; i < kreetures.Count; i++)
		{
			if (i == selectedMember)
				memberSlots[i].SetSelected(true);
			else
				memberSlots[i].SetSelected(false);
		}
	}

	public IEnumerator ShowPartyScreen()
	{
        this.gameObject.SetActive(true);
        yield return this.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), .25f);
    }

    public IEnumerator HidePartyScreen()
    { 
        yield return this.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -100), .25f);
        this.gameObject.SetActive(false);
    }

	public void ShowIfTmIsUsable(LearnableItem learnableItem)
	{
		for (int i = 0; i < kreetures.Count; i++)
		{
			string message = learnableItem.CanBeTaught(kreetures[i]) ? "ABLE!" : "NOT ABLE!";
			memberSlots[i].SetMessage(message);
		}
	}

	public void ClearMemberSlotMessages()
	{
		for (int i = 0; i < kreetures.Count; i++)
		{
			memberSlots[i].SetMessage("");
		}
	}

	public void SetMessageText(string message)
	{
		messageText.text = message;
	}
}
