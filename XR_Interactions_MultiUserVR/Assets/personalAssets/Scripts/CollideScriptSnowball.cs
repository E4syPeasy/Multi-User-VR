using UnityEngine;
using Photon.Pun;

public class CollideScriptSnowball : MonoBehaviourPun, IPunObservable
{
    PhotonView GSCphotonView;
    int collsionCounter;
    PhotonView thisPV;
    bool sbDestroyed;
    Rigidbody rb;
    Vector3 latestPos;
    Quaternion latestRot;
    Vector3 velocity;
    Vector3 angularVelocity;
    bool valuesReceived = false;
    public bool SnowballGrabbed; 

    private void Start()
    {
        GSCphotonView = GameSetupController.GSC.GetComponent<PhotonView>();
        thisPV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        //collsionCounter++;
        //Debug.Log("collisionCounter: " + collsionCounter);

        // ignore collisions while spawning
        if (!SnowballGrabbed) // previously   collsionCounter < 2
        {
            return;
        }
        
        if (!sbDestroyed && collision.gameObject.tag == "Avatar1")
        {
            //when player1 gets hit -> add a point to player2
            GSCphotonView.RPC("UpdatePlayer2Score", RpcTarget.All);
            destroySnowball();
            sbDestroyed = true;
        }

        else if (!sbDestroyed && collision.gameObject.tag == "Avatar2") //when player2 gets hit -> add a point to player1
        {
            GSCphotonView.RPC("UpdatePlayer1Score", RpcTarget.All);
            destroySnowball();
            sbDestroyed = true;
        }
        else if(!sbDestroyed)
        {
            //should be destoyed when it hits ground/fence (but not own Avatar etc.)
            destroySnowball();
            sbDestroyed = true;
        }
    }
    
    // snowball positions only get updated when grabbed/thrown
    public void OnSnowballGrabbed()
    {
        Debug.Log("SnowballGrabbed: " + SnowballGrabbed);
        SnowballGrabbed = true;
    }

    public void destroySnowball()
    {
        //this destroys LOCALLY ONLY
        //Destroy(this.gameObject, 0.1f);
        
        Debug.Log("Collide-script: try destroy" + thisPV);
        //only master destroys (network-)instantiated object associated with photonView (for all players)
        // ^ only for scene/room-objects

        if (PhotonNetwork.IsMasterClient)
        {
            GameSetupController.GSC.endOfGameSBThrownP1++;
        }
        else
        {
            GameSetupController.GSC.endOfGameSBThrownP2++;
        }
        GameSetupController.GSC.SpawnSnowballAtRandomLoc();
        Debug.Log("Snowball-counter (CollideScript):" + GameSetupController.GSC.snowballsSpawned);

        if (thisPV.IsMine)
        {
            //PhotonNetwork.Destroy(thisPV);
            PhotonNetwork.Destroy(gameObject);
        }

    }

    //once per frame
    void Update()
    {
        if (!thisPV.IsMine && valuesReceived)
        {
            //Update Object position and Rigidbody parameters
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
        }
    }

    //updates synchronizes Snowballs for all players network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (SnowballGrabbed)
            {
                //We own this player: send the others our data
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(rb.velocity);
                stream.SendNext(rb.angularVelocity);
            }
           
        }
        else
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            angularVelocity = (Vector3)stream.ReceiveNext();

            valuesReceived = true;
        }
    }
}