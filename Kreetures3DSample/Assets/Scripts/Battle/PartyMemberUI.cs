using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    Kreeture _kreeture;

    public void SetData(Kreeture kreeture)
    {
        _kreeture = kreeture;

        nameText.text = kreeture.Base.Name;
        levelText.text = "Lvl " + kreeture.Level;
        hpBar.SetHP((float)kreeture.HP / kreeture.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if(GlobalSettings.i != null)
		{
            if (selected)
                nameText.color = GlobalSettings.i.HighlightedColor;
            else
                nameText.color = Color.white;
        }        
    }
}
