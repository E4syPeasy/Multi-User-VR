using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

// this class handles most of the menu-scene (selecting levels, updating players, updating Canvas)
public class DSWaitingRoomController : MonoBehaviourPunCallbacks
{
    private PhotonView myPV;
    private int playerCount;
    private int roomSize; // for the study the size is 2 
    bool roomFull;
    bool levelSelected;

    public int multiplayerSceneIndex; // changed through ingame-buttons
    public TextMeshProUGUI playerCounterText;
    public TextMeshProUGUI SelectedLevelText;
    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        //init variables
        myPV = GetComponent<PhotonView>();
        PlayerCountUpdate();
    }
    

    //called when new player joins room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerCountUpdate();
    }

    void PlayerCountUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        playerCounterText.SetText(playerCount + " of " + roomSize + System.Environment.NewLine + "players connected");

        if (playerCount == roomSize)
        {
            myPV.RPC("readyToStartRPC", RpcTarget.MasterClient);
        }
        else //false when player leaves lobby
        {
            myPV.RPC("notReadyToStartRPC", RpcTarget.All);
        }
    }
    //called when new player leaves room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerCountUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && levelSelected && roomFull)
        {
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
    }
    
    //loads selected multiplayer-scene and prevents new player-joins
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false; 
            PhotonNetwork.LoadLevel(multiplayerSceneIndex);
        }
    }


    //dropdowns don't work -> using buttons for now
    //sends scene-changes to every client
    public void HandleInputData(int val)
    {
        if (val == 101)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 2);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
        else if (val == 102)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 3);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
        else if (val == 1)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 4);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
        else if (val == 2)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 5);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
        else if(val == 3)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 6);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
        else if (val == 4)
        {
            myPV.RPC("LevelSelected", RpcTarget.MasterClient, 7);
            myPV.RPC("UpdateCanvas", RpcTarget.All, val);
            Debug.Log("selected: " + val);
        }
    }
    
    public void DelayCancel()
    {
        myPV.RPC("SelectionCanceled", RpcTarget.All);
        myPV.RPC("UpdateCanvasCancel", RpcTarget.All);
        Debug.Log("canceled ");
    }

    [PunRPC]
    private void readyToStartRPC()
    {
        roomFull = true;
    }

    [PunRPC]
    private void notReadyToStartRPC()
    {
        roomFull = false;
    }

    //send master update so he can start the Game ...
    [PunRPC]
    private void LevelSelected(int sceneIndex)
    {
        //SelectedLevelText.SetText("Selected Level: " + level);
        multiplayerSceneIndex = sceneIndex;
        levelSelected = true;
    }

    [PunRPC]
    private void SelectionCanceled()
    {
        levelSelected = false;
    }

    [PunRPC]
    private void UpdateCanvas(int level)
    {
        SelectedLevelText.SetText("Selected Level: " + level);
    }

    [PunRPC]
    private void UpdateCanvasCancel()
    {
        SelectedLevelText.SetText("no Level selected ");
    }
}
