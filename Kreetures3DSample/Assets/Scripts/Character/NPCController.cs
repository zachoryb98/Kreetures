using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector3> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    Quest activeQuest;

    private PlayerInput playerControls;

    ItemGiver itemGiver;
    KreetureGiver kreetureGiver;

    private void Awake()
    {
        playerControls = new PlayerInput();
        itemGiver = GetComponent<ItemGiver>();
        kreetureGiver = GetComponent<KreetureGiver>();
    }

    public IEnumerator Interact()
    {
        if (questToComplete != null)
        {
            var quest = new Quest(questToComplete);
            yield return quest.CompleteQuest(GameManager.Instance.playerController.gameObject.transform);
            questToComplete = null;

            Debug.Log($"{quest.Base.QuestName} completed");
        }

        if (itemGiver != null && itemGiver.CanBeGiven())
        {
            yield return itemGiver.GiveItem(GameManager.Instance.playerController);
        }
        else if (kreetureGiver != null && kreetureGiver.CanBeGiven())
        {
            yield return kreetureGiver.GiveKreeture(GameManager.Instance.playerController);
        }
        //Start quest
        else if (questToStart != null)
        {
            activeQuest = new Quest(questToStart);
            yield return activeQuest.StartQuest();
            questToStart = null;

            if (activeQuest.CanBeCompleted())
            {
                yield return activeQuest.CompleteQuest(GameManager.Instance.playerController.transform);
                activeQuest = null;
            }
        }
        else if (activeQuest != null)
        {
            if (activeQuest.CanBeCompleted())
            {
                yield return activeQuest.CompleteQuest(GameManager.Instance.playerController.transform);
                activeQuest = null;
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialogue);
            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
        }

        state = NPCState.Idle;
    }

    private void OnEnable()
    {
        playerControls.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.PlayerControls.Disable();
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();

        if (questToComplete != null)
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;

            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking }
