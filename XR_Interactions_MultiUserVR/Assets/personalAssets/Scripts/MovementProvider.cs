using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// basic movement from VR with Andrew
// https://github.com/C-Through/VR-XRToolkitLocomotion/blob/master/Assets/Scripts/MovementProvider.cs
public class MovementProvider : LocomotionProvider
{
    PhotonView photonView;

    public List<XRController> controllers = null;
    
    public GameObject leftController;
    public GameObject rightController;
    public GameObject Avatar;
    public Renderer[] allRenderer;

    public float speed = 1.1f;

    private CharacterController characterController = null;
    private GameObject head = null;

    protected override void Awake()
    {
        photonView = GetComponent<PhotonView>();
       // photonView.ObservedComponents.Add(this);
        
        if (photonView.IsMine)
        {
            characterController = GetComponent<CharacterController>();
            head = GetComponent<XRRig>().cameraGameObject;
            //disable collider to prevent hitting yourself 
            Avatar.GetComponent<CapsuleCollider>().enabled = false;
            //disable own avatar for clear FoV
            // Avatar.GetComponent<MeshRenderer>().enabled = false;
            
            //disable all renderers (set in editor/avatar-specific)
            foreach (Renderer r in allRenderer)
            {
                r.enabled = false;
            }

            characterController.enabled = true;
            //enable VR-Rig for local player
            head.SetActive(true);
            leftController.SetActive(true);
            rightController.SetActive(true);

        }
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            PositionController();
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine) 
        {
            PositionController();
            CheckForInput();
        }
    }

    private void PositionController()
    {
        // Get the head in local, playspace ground
        float headHeight = Mathf.Clamp(head.transform.localPosition.y, 0.5f, 2.0f);
        characterController.height = headHeight;

        // Cut in half, add skin
        Vector3 newCenter = Vector3.zero;
        newCenter.y = characterController.height / 2;
        newCenter.y += characterController.skinWidth;

        // move the capsule in local space as well
        newCenter.x = head.transform.localPosition.x;
        newCenter.z = head.transform.localPosition.z;

        // Apply
        characterController.center = newCenter;
    }

    private void CheckForInput()
    {
        foreach(XRController controller in controllers)
        {
            if (controller.enableInputActions)
                CheckForMovement(controller.inputDevice);
        }
    }

    private void CheckForMovement(InputDevice device)
    {
        if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 position))
        StartMove(position);
    }

    private void StartMove(Vector2 position)
    {
        // Apply the touch position to the head's forward Vector
        Vector3 direction = new Vector3(position.x, 0, position.y);
        Vector3 headRotation = new Vector3(0, head.transform.eulerAngles.y, 0);

        // Rotate the input direction by the horizontal head rotation
        direction = Quaternion.Euler(headRotation) * direction;

        // Apply speed and move
        Vector3 movement = direction * speed;
        characterController.Move(movement * Time.deltaTime);
    }
    
}