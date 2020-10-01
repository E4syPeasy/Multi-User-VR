using System;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

namespace UnityEngine.XR.Interaction.Toolkit
{

    public class CustomTeleportationSetup : MonoBehaviour, IPunObservable
    {
        PhotonView photonView;
        
        public GameObject leftController;
        public GameObject rightController;
        public GameObject Avatar;
        public Renderer[] allRenderer;

        void Awake()
        {
            photonView = GetComponent<PhotonView>();
            photonView.ObservedComponents.Add(this);

            if (photonView.IsMine)
            {
                GetComponent<XRRig>().cameraGameObject.SetActive(true);
                // GetComponent<SnapTurnProvider>().enabled = true; //not necessary 
                GetComponent<LocomotionSystem>().enabled = true;
                GetComponent<TeleportationProvider>().enabled = true;
                //disable Collider to prevent hitting yourself 
                Avatar.GetComponent<CapsuleCollider>().enabled = false;

                //disable all renderers (set in editor/avatar-specific)
                foreach (Renderer r in allRenderer)
                {
                    r.enabled = false;
                }

                leftController.SetActive(true);
                rightController.SetActive(true);
            }
        }

        //TODO: read photon-doc on IPunObservable -> momentan doppelt (photonTransformView in editor)
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
          //      stream.SendNext(transform.position); //position of the character
          //      stream.SendNext(transform.rotation); //rotation of the character
            }
            else
            {
          //      // Network player, receive data
          //      Vector3 syncPosition = (Vector3)stream.ReceiveNext();
           //     Quaternion syncRotation = (Quaternion)stream.ReceiveNext();
            }
        }



    }
}

