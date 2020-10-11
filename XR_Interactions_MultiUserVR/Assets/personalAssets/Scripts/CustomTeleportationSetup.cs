using System;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

namespace UnityEngine.XR.Interaction.Toolkit
{

    public class CustomTeleportationSetup : MonoBehaviour
    {
        PhotonView photonView;
        
        public GameObject leftController;
        public GameObject rightController;
        public GameObject Avatar;
        public Renderer[] allRenderer;

        void Awake()
        {
            photonView = GetComponent<PhotonView>();

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


        //triggered with button-Press (leftHand controller)
        public void teleportCounter()
        {
            GameSetupController.GSC.localMovement++;
           // Debug.Log("localMovement (tp): " + GameSetupController.GSC.localMovement);
        }

    }
}

