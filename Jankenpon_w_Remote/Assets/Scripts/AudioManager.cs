using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] AudioSource BGM;

    [Header("SFX")]
    [SerializeField] AudioSource Win;
    [SerializeField] AudioSource Lose;
    [SerializeField] AudioSource ChooseCard;
    [SerializeField] AudioSource Attack;
    [SerializeField] AudioSource Slashed;
    
    float bgm_volume;
    
    private void Start() {
        bgm_volume = BGM.volume;
    }
    
    public void PlayWin()
    {
        BGM.DOKill();
        BGM.DOFade(0.1f, 0.5f).SetDelay(1);
        Win.PlayDelayed(1);
        // BGM.DOFade(0.5f, 3f).SetDelay(2);
    }

    public void PlayLose()
    {
        BGM.DOKill();
        BGM.DOFade(0.1f, 0.5f).SetDelay(1);
        Lose.PlayDelayed(1);
        // BGM.DOFade(0.5f, 3f).SetDelay(2);
    }

    public void PlayChooseCard()
    {
        ChooseCard.Play();
    }
    public void PlayAttack()
    {
        BGM.DOFade(0.3f, 0.1f);
        Attack.Play();
        BGM.DOFade(bgm_volume, 1f).SetDelay(0.5f);
    }

    public void PlaySlashed()
    {
        BGM.DOFade(0.3f, 0.1f);
        Slashed.Play();
        BGM.DOFade(bgm_volume, 1f).SetDelay(0.5f);
    }
}
