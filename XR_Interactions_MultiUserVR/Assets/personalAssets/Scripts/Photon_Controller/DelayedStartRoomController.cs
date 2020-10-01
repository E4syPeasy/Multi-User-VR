using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class DelayedStartRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public int waitingRoomSceneIndex; // multiplayer-scene-index - set in inspector

    //needed because this script uses callbacks triggered from LobbyController
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    //callback when created/joined room
    //triggered from LobbyController
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        SceneManager.LoadScene(waitingRoomSceneIndex);
    }
}
