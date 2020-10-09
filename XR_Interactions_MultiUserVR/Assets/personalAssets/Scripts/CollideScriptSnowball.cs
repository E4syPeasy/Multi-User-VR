using UnityEngine;
using Photon.Pun;

public class CollideScriptSnowball : MonoBehaviourPun, IPunObservable
{
    PhotonView GSCphotonView;
    int collsionCounter;
    PhotonView thisPV;

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
        }
    }

    //Transfer PhotonView of obj/Rigidbody to our local player
    public void OnSnowballGrabbed()
    {
        if (!photonView.IsMine)
        {
            thisPV.TransferOwnership(PhotonNetwork.LocalPlayer); //photonView.Tr...
        }

        SnowballGrabbed = true;
    }

    public void destroySnowball()
    {
        //this destroys LOCALLY ONLY
        //Destroy(this.gameObject, 0.1f);
        
        Debug.Log("Collide-script: try destroy" + thisPV);
        //only master destroys (network-)instantiated object associated with photonView (for all players)
        GSCphotonView.RPC("DeleteSnowball", RpcTarget.MasterClient, thisPV);

        GameSetupController.GSC.snowballsSpawned--;
        Debug.Log("Snowball-counter (CollideScript):" + GameSetupController.GSC.snowballsSpawned);

    }

    //once per frame
    void Update()
    {
        if (!photonView.IsMine && valuesReceived)
        {
            //Update Object position and Rigidbody parameters
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
        }
    }


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