using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image image;
    public Sprite hideSprite;
    public Sprite sprite;
    public Attack AttackValue;
    public CardPlayer player;
    public Vector2 OriginalPosition;
    public AudioManager audioManager;
    Vector3 originalScale;
    Color originalColor;

    bool isClickable = true;

    private void Start()
    {
        OriginalPosition = this.transform.position;
        originalScale = this.transform.localScale;
        originalColor = GetComponent<Image>().color;
    }

    public void OnClick()
    {
        if (isClickable){
            audioManager.PlayChooseCard();
            player.SetChoosenCard(this);
        }
    }

    public void Reset()
    {
        transform.position = OriginalPosition;
        transform.localScale = originalScale;
        GetComponent<Image>().color = originalColor;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }

    public void Show()
    {
        image.sprite = sprite;
    }

    public void Hide()
    {
        image.sprite = hideSprite;
    }



    // ----------- Contoh Tweening
    // private void Start()
    // {
    //     startPosition = this.transform.position;
    //     var seq = DOTween.Sequence();
    //     seq.Append(transform.DOMove(atkPoskRef.position, 1));
    //     seq.Append(transform.DOMove(startPosition, 1));

    // }

    // ---------- Tweening Manual
    // float timer = 0;

    // private void Update()
    // {
    //     if (timer < 1)
    //     {
    //         var newX = Linear(startPosition.x, atkPoskRef.position.x, timer);
    //         var newY = Linear(startPosition.y, atkPoskRef.position.y, timer);
    //         this.transform.position = new Vector2(newX, newY);
    //         timer += Time.deltaTime;
    //     }
    //     else
    //     {
    //         timer = 0;
    //     }
    // }

    // private float Linear(float start, float end, float time)
    // {
    //     return (end - start) * time + start;
    // }
}
