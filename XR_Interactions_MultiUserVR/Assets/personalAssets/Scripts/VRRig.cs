using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//currently not used
[System.Serializable]
public class VRMap
{
    public Transform rigTarget;
    public Transform vrTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    // set postion and rotation of the rig-target
    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}


public class VRRig : MonoBehaviour, IPunObservable
{
    public Transform headConstraint;
    public Vector3 headBodyOffset;
    public float turnSmoothness; //set in editor (currently 5)
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        //calculate initial difference between head and body
        //headBodyOffset = transform.position - headConstraint.position;
        headBodyOffset.y = -0.6f;
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            //move body according to head-position
            transform.position = headConstraint.position + headBodyOffset;

            //aline rotation (only one y-axis)
            //lerp for delay, feels more natural 
            transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized, Time.deltaTime * turnSmoothness);

            // update postions, positions should now always match
            head.Map();
            leftHand.Map();
            rightHand.Map();
        }
    }

    //TODO: photon-doc on IPunObservable: https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-networking
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
             // We own this player: send the others our data
            //stream.SendNext(rightHand);
            //stream.SendNext(leftHand); 
            // stream.SendNext(head);
            //stream.SendNext(headConstraint);
        }
        else
        {
            // Network player, receive data
           // rightHand = (VRMap)stream.ReceiveNext();
           // leftHand = (VRMap)stream.ReceiveNext();
          //  head = (VRMap)stream.ReceiveNext();
          //  headConstraint = (Transform)stream.ReceiveNext();
        }
    }
}
