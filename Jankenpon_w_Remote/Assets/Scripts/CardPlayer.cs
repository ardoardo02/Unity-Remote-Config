using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class CardPlayer : MonoBehaviour
{
    public Transform atkPoskRef;
    public Card choosenCard;
    public HealthBar healthBar;
    public TMP_Text NickName;
    public TMP_Text healthText;
    public float Health;
    public float MaxHealth;
    public Card[] cards;
    public bool alwaysShowCards;

    private Tweener animationTween;

    public void Start()
    {
        Health = MaxHealth;
        if(!alwaysShowCards)
            HideCards();
    }

    public Attack? AttackValue
    {
        get => choosenCard == null ? null : choosenCard.AttackValue;
    }

    public void Reset()
    {
        if (choosenCard != null)
        {
            choosenCard.Reset();
        }

        choosenCard = null;

        if(!alwaysShowCards)
            HideCards();
    }

    public void SetChoosenCard(Card newCard)
    {
        if (choosenCard != null)
        {
            choosenCard.Reset();
        }

        choosenCard = newCard;
        if(alwaysShowCards)
            choosenCard.transform.DOScale(choosenCard.transform.localScale * 1.2f, 0.2f);
    }

    public void ChangeHealth(float amount)
    {
        Health += amount;
        Health = Math.Clamp(Health, 0, 100);

        // healthbar
        healthBar.UpdateBar(Health / MaxHealth);

        //text
        healthText.text = Health + " / " + MaxHealth;
    }

    public void AnimateAttack()
    {
        animationTween = choosenCard.transform.DOMove(atkPoskRef.position, 1);
        if(!alwaysShowCards)
            ShowCards();
    }

    public void AnimateDraw()
    {
        animationTween = choosenCard.transform
            .DOMove(choosenCard.OriginalPosition, 1)
            .SetEase(Ease.InBack)
            .SetDelay(0.2f);
    }

    public void AnimateDamage()
    {
        var image = choosenCard.GetComponent<Image>();
        animationTween = image
            .DOColor(Color.red, 0.1f)
            .SetLoops(3, LoopType.Yoyo)
            .SetDelay(0.2f);
    }

    public bool IsAnimating()
    {
        return animationTween.IsActive();
    }

    public void IsClickable(bool value)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        foreach (var card in cards)
        {
            card.SetClickable(value);
        }
    }
    
    public void HideCards()
    {
        if(alwaysShowCards)
            return;
        
        foreach (var card in cards)
        {
            card.Hide();
        }
    }

    public void ShowCards()
    {
        if(alwaysShowCards)
            return;
        
        foreach (var card in cards)
        {
            card.Show();
        }
    }
}
