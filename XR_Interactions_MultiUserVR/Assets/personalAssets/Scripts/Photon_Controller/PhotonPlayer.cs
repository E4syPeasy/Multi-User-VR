using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//currently not used (handled in GameSetupController)
public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;
    public GameObject myAvatar;

    //Creates a network-player-obj when a new player joins
    private void CreatePlayer(int spawnPoint)
    {
        //creates PlayerAvatar ("PhotonPlayer") at a spawn-point
        Debug.Log("Creating Player");
        //TODO: add "PhotonPlayer" with VR-rig
        myAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "TestPhotonPlayer"), 
            GameSetupController.GSC.spawnPoints[spawnPoint].position, GameSetupController.GSC.spawnPoints[spawnPoint].rotation);
    }

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            //CreateTestPlayer();

            if (PhotonNetwork.CountOfPlayers == 0)
            {
                CreatePlayer(0); //spawn at first spawn-point (first join)
                Debug.Log("PhotonNetwork.CountOfPlayers");
            }
            else
            {
                CreatePlayer(1); //spawn at second spawn-point (for everyone else)
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
