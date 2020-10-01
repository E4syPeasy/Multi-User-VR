using UnityEngine;
using Photon.Pun;

public class CollideScriptSnowball : MonoBehaviour
{
    PhotonView GSCphotonView;
    int collsionCounter;
   // PhotonView thisPV;
   
    private void Start()
    {
        GSCphotonView = GameSetupController.GSC.GetComponent<PhotonView>();
       // thisPV = GetComponent<PhotonView>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        //ignore collisions while spawning - apparently 2 (only workaround) 
        //TODO find clean way
        if (collsionCounter < 2)
        {
            collsionCounter++;
            //Debug.Log("collision" + collsionCounter);
            
            return;
        }

        if (collision.gameObject.tag == "Avatar1")
        {
            //when player1 gets hit -> add a point to player2
            GSCphotonView.RPC("UpdatePlayer2Score", RpcTarget.All);
            destroySnowball();
        }

        else if (collision.gameObject.tag == "Avatar2") //when player2 gets hit -> add a point to player1
        {
            GSCphotonView.RPC("UpdatePlayer1Score", RpcTarget.All);
            destroySnowball();
        }
        else
        {
            //should be destoyed when it hits ground/fence (but not own Avatar etc.)
            destroySnowball();
            // GSCphotonView.RPC("DeleteSnowball", RpcTarget.MasterClient, thisPV);
        }
    }

    public void destroySnowball()
    {
        //this destroys LOCALLY ONLY
        Destroy(this.gameObject, 0.1f);

        GameSetupController.GSC.snowballsSpawned--;
        Debug.Log("Snowball-counter:" + GameSetupController.GSC.snowballsSpawned);

        // Debug.Log("Collide-script: try destroy" + thisPV);
        //only master destroys (network-)instantiated object associated with photonView (for all players)
        // GSCphotonView.RPC("DeleteSnowball", RpcTarget.MasterClient, thisPV);
    }
}