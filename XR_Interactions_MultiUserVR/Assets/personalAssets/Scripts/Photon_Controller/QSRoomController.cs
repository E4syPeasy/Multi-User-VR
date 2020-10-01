using UnityEngine;
using Photon.Pun;

public class QSRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public int SceneIndex; // multiplayer-scene-index - set in inspector

    //needed because this script uses callbacks triggered from QSLobbyController
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    //loads into multiplayer(-scene)
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting the Game");

            // players should always join existing room because of AutoSync 
            //scene index is currently set to 1
            PhotonNetwork.LoadLevel(SceneIndex); 
        }
    }

    //callback when created/joined room
    //triggered from QSLobbyController
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        StartGame();
    }
}
