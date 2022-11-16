using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField newRoomInputField;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Button startGameButton;
    [SerializeField] GameObject roomPanel;

    [SerializeField] GameObject roomListObject;
    [SerializeField] RoomItem roomItemPrefab;
    List<RoomItem> roomItemList = new List<RoomItem>();

    [SerializeField] GameObject playerListObject;
    [SerializeField] PlayerItem playerItemPrefab;
    List<PlayerItem> playerItemList = new List<PlayerItem>();
    Dictionary<string, RoomInfo> roomInfoCache = new Dictionary<string, RoomInfo>();

    private void Start() {
        feedbackText.text = "";
        PhotonNetwork.JoinLobby();
    }

    public void ClickCreateRoom()
    {
        feedbackText.text = "";

        if(newRoomInputField.text.Length < 3){
            feedbackText.text = "Room name min 3 Characters";
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(newRoomInputField.text, roomOptions);
    }

    public void ClickStartGame(string levelName)
    {
        if(PhotonNetwork.IsMasterClient){
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(levelName);
        }
    }

    public void ClickLeaveRoom()
    {
        StartCoroutine(BackToLobbyCR());
    }

    IEnumerator BackToLobbyCR()
    {
        PhotonNetwork.LeaveRoom();
        while(PhotonNetwork.InRoom || PhotonNetwork.IsConnectedAndReady == false)
            yield return null;
        
        PhotonNetwork.JoinLobby();
        roomPanel.SetActive(false);
    }

    public void JoinRoom(string roomName){
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Created room" + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        feedbackText.text = "Joined room: " + PhotonNetwork.CurrentRoom.Name;

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        roomPanel.SetActive(true);

        UpdatePlayerList();
        SetStartGameButton();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        SetStartGameButton();
    }

    private void SetStartGameButton()
    {
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        startGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    private void UpdatePlayerList()
    {
        foreach (var item in playerItemList)
        {
            Destroy(item.gameObject);
        }
        playerItemList.Clear();

        foreach (var (id, player) in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerListObject.transform);
            newPlayerItem.Set(player);
            playerItemList.Add(newPlayerItem);

            if(player == PhotonNetwork.LocalPlayer)
                newPlayerItem.transform.SetAsFirstSibling();
        }

        SetStartGameButton();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Updating room...");
        foreach (var roomInfo in roomList)
        {
            roomInfoCache[roomInfo.Name] = roomInfo;
        }

        foreach (var item in this.roomItemList)
        {
            Destroy(item.gameObject);
        }

        roomItemList.Clear();

        var roomInfoList = new List<RoomInfo>(roomInfoCache.Count);

        foreach (var roomInfo in roomInfoCache.Values)
        {
            if(roomInfo.IsOpen)
                roomInfoList.Add(roomInfo);
        }

        foreach (var roomInfo in roomInfoCache.Values)
        {
            if(!roomInfo.IsOpen)
                roomInfoList.Add(roomInfo);
        }

        foreach (var roomInfo in roomInfoList)
        {
            if(roomInfo.MaxPlayers == 0 || roomInfo.IsVisible == false)
                continue;

            RoomItem newRoomItem = Instantiate(roomItemPrefab, roomListObject.transform);
            newRoomItem.Set(this, roomInfo);
            this.roomItemList.Add(newRoomItem);
        }
    }
}
