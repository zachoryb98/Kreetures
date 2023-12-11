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

    /// <summary>
    /// Initializer Method
    /// </summary>
    /// <param name="kreeture"></param>
    public void Init(Kreeture kreeture)
    {
        _kreeture = kreeture;
        UpdateData();

        //Observer pattern to update health
        _kreeture.OnHPChanged += UpdateData;
    }

    /// <summary>
    /// Updates Party Member UI information for party screen
    /// </summary>
    void UpdateData()
    {
        nameText.text = _kreeture.Base.Name;
        levelText.text = "Lvl " + _kreeture.Level;
        hpBar.SetHP((float)_kreeture.HP / _kreeture.MaxHp);
    }

    /// <summary>
    /// Sets selected color
    /// </summary>
    /// <param name="selected"></param>
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
