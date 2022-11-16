using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using DG.Tweening;

public class CardGameManager : MonoBehaviour, IOnEventCallback
{
    public GameObject netPlayerPrefab;
    public CardPlayer P1;
    public CardPlayer P2;
    public int restoreValue = 5;
    public int damageValue = 15;
    public GameState State, NextState = GameState.NetPlayersInit;
    public GameObject gameOverPanel;
    public Transform bgImage;
    public TMP_Text winnerText;
    public TMP_Text pingText;

    private CardPlayer damagedPlayer;
    private CardPlayer winner;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private bool online;

    public List<int> syncReadyPlayers = new List<int>(2);
    public enum GameState
    {
        SyncStats,
        NetPlayersInit,
        ChooseAttack,
        Attacks,
        Damages,
        Draw,
        GameOver,
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        if(online){
            PhotonNetwork.Instantiate(netPlayerPrefab.name, Vector3.zero, Quaternion.identity);
            StartCoroutine(PingCoroutine());
            State = GameState.NetPlayersInit;
            NextState = GameState.NetPlayersInit;
        }else{
            State = GameState.ChooseAttack;
        }
    }

    private void Update()
    {
        // ChooseAttack
        switch (State)
        {
            case GameState.SyncStats:
                if(syncReadyPlayers.Count == 2){
                    syncReadyPlayers.Clear();
                    State = NextState;
                }
                break;
            case GameState.NetPlayersInit:
                if(CardNetPlayer.NetPlayers.Count == 2){
                    foreach (var netPlayer in CardNetPlayer.NetPlayers)
                    {
                        if(netPlayer.photonView.IsMine)
                            netPlayer.Set(P1);
                        else
                            netPlayer.Set(P2);
                    }
                    ChangeState(GameState.ChooseAttack);
                }
                break;
            case GameState.ChooseAttack:
                if (P1.AttackValue != null && P2.AttackValue != null)
                {
                    P1.AnimateAttack();
                    P2.AnimateAttack();
                    P1.IsClickable(false);
                    P2.IsClickable(false);
                    ChangeState(GameState.Attacks);
                }
                break;

            case GameState.Attacks:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    damagedPlayer = GetDamagedPlayer();

                    if (damagedPlayer != null)
                    {
                        damagedPlayer.AnimateDamage();
                        ChangeState(GameState.Damages);
                    }
                    else
                    {
                        P1.AnimateDraw();
                        P2.AnimateDraw();
                        ChangeState(GameState.Draw);
                    }
                }
                break;

            case GameState.Damages:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    bgImage.DOShakePosition(0.5f, 100, 100, 90, true);

                    // Hitung darah
                    if (damagedPlayer == P1)
                    {
                        audioManager.PlaySlashed();
                        P1.ChangeHealth(-damageValue);
                        P2.ChangeHealth(restoreValue);
                    }
                    else
                    {
                        audioManager.PlayAttack();
                        P1.ChangeHealth(restoreValue);
                        P2.ChangeHealth(-damageValue);
                    }

                    var winner = GetWinner();

                    if (winner == null)
                    {
                        ResetPlayers();
                        P1.IsClickable(true);
                        P2.IsClickable(true);
                        ChangeState(GameState.ChooseAttack);
                    }
                    else
                    {
                        if(winner == P1)
                            audioManager.PlayWin();
                        else
                            audioManager.PlayLose();

                        gameOverPanel.SetActive(true);
                        winnerText.text = winner == P1 ? $"{P1.NickName.text} is win!" : $"{P2.NickName.text} is win!";
                        ResetPlayers();
                        ChangeState(GameState.GameOver);
                    }
                }
                break;

            case GameState.Draw:
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    ResetPlayers();
                    P1.IsClickable(true);
                    P2.IsClickable(true);
                    ChangeState(GameState.ChooseAttack);
                }
                break;
        }
    }
    private void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void ChangeState(GameState newState)
    {
        if(!online){
            State = newState;
            return;
        }

        if(this.NextState == newState)
            return;
        
        var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        var raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(1, actorNum, raiseEventOptions, SendOptions.SendReliable);
        this.State = GameState.SyncStats;
        this.NextState = newState;
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code == 1){
            var actorNum = (int)photonEvent.CustomData;

            if(syncReadyPlayers.Contains(actorNum) == false)
                syncReadyPlayers.Add(actorNum);
        }
    }

    IEnumerator PingCoroutine()
    {
        var wait = new WaitForSeconds(1);
        while(true){
            pingText.text = "Ping: " + PhotonNetwork.GetPing() + "ms";
            yield return wait;
        }
    }

    private void ResetPlayers()
    {
        damagedPlayer = null;
        P1.Reset();
        P2.Reset();
    }
    private CardPlayer GetDamagedPlayer()
    {
        Attack? PlayerAtk1 = P1.AttackValue;
        Attack? PlayerAtk2 = P2.AttackValue;

        if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Paper)
            return P1;
        else if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Scissor)
            return P2;
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Rock)
            return P2;
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Scissor)
            return P1;
        else if (PlayerAtk1 == Attack.Scissor && PlayerAtk2 == Attack.Rock)
            return P1;
        else if (PlayerAtk1 == Attack.Scissor && PlayerAtk2 == Attack.Paper)
            return P2;

        return null;
    }

    private CardPlayer GetWinner()
    {
        if (P1.Health == 0)
        {
            return P2;
        }
        else if (P2.Health == 0)
        {
            return P1;
        }
        else
        {
            return null;
        }
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
