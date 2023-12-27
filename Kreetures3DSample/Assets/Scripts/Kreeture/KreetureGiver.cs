using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KreetureGiver : MonoBehaviour, ISavable
{
    [SerializeField] Kreeture kreetureToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveKreeture(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        kreetureToGive.Init();
        player.GetComponent<KreetureParty>().AddKreeture(kreetureToGive);

        used = true;

        string dialogText = $"{player.name} received {kreetureToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return kreetureToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
