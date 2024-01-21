using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingNPC : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        yield return Fader.i.FadeIn(0.5f);

        var playerParty = player.GetComponent<KreetureParty>();
        playerParty.Kreetures.ForEach(p => p.Heal());
        playerParty.PartyUpdated();

        yield return Fader.i.FadeOut(0.5f);
    }
}
