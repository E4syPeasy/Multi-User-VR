using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public GameObject quickStartButton;
    [SerializeField]
    public GameObject quickCancelButton;
    [SerializeField]
    private int RoomSize;

    //callback after first connection
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //use same scene as master-client
        quickStartButton.SetActive(true);
    }

    //for StartButton
    public void QuickStart()
    {
        quickStartButton.SetActive(false);
        quickCancelButton.SetActive(true);

        // try join existing room
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Start");
    }

    //when no room is available or failed to join
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Joining failed ");
        CreateRoom(); 
    }

    // creates new Room with random name
    void CreateRoom()
    {
        Debug.Log("Created new room");

        int randomRoomName = Random.Range(0, 10000); // random number as name
        RoomOptions roomOpt = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2}; // 2 max players for now
        PhotonNetwork.CreateRoom("Room" + randomRoomName, roomOpt); // try to create new room with given opt./name

        Debug.Log("Room-number:" + randomRoomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed creating new room ... try again");
        CreateRoom(); //retrying with other name
    }

    //for CancelButton
    public void QuickCancel()
    {
        quickCancelButton.SetActive(false);
        quickStartButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }
}
