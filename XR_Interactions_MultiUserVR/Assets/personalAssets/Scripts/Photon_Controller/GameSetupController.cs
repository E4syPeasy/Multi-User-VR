using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;

public class GameSetupController : MonoBehaviour
{
    public int level; //set in editor (used to spawn different player-prefabs in every lvl)
    public Transform[] spawnPoints = new Transform[2];
    public static GameSetupController GSC;
    public int player1Score;
    public int player2Score;
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public GameObject groundPlayer1;
    public GameObject groundPlayer2;
    public GameObject myPlayer1;
    public Animator animator;
    public GameObject myPlayer2;
    PhotonView photonView;

    public int snowballsSpawned;
    public int maxSnowballs;
    public GameObject snowball;
    Coroutine player1Coroutine;
    Coroutine player2Coroutine;
    public float xArea; //Area for snowball-spawns
    public float zArea;
    public float spawnWait; //time between sb spawns
    public float startWait; //time before sb start spawning
    public bool pauseCoroutine;
    public TextMeshProUGUI timerText;
    public float matchLength; //in seconds
    bool gameEnded;

    private void Awake()
    {
        GSC = this;
        photonView = GetComponent<PhotonView>();
        gameEnded = false;

        player1Score = 0;
        player2Score = 0;
        snowballsSpawned = 0;
        maxSnowballs = 30;
        xArea = 15.0f; //Area for sb spawns (+/- 15)
        zArea = 4.0f;
        spawnWait = 0.5f; //time between sb spawns
        startWait = 0.0f; //time before sb start spawning
    }

    // Start is called before the first frame update
    void Start()
    {
        //spawn at first spawn-point (only first player who joined lobby)
        if (PhotonNetwork.IsMasterClient) //PhotonNetwork.CountOfPlayers == 1
        {
            //creates PlayerAvatar ("PhotonPlayer") at a spawn-point 1
            Debug.Log("Creating Player1");
            CreatePlayerOne(level);
            Debug.Log("Player Count: " + PhotonNetwork.CountOfPlayers);
            
            //start spawning snowballs for player1 (local only)
            //TODO normally better to instantiate over network ... PROBLEM: network obj do not work with XR-interactable/grabbing
            player1Coroutine = StartCoroutine(Spawner(spawnPoints[0].position));
        }
        else //spawn at second spawn-point (for everyone else)
        {
            //creates PlayerAvatar ("PhotonPlayer") at a spawn-point 2
            Debug.Log("Creating Player2");

            if (level == 0)
            {
                CreatePlayerTwoTutorial(); // s_teleport
            }
            else
            {
                CreatePlayerTwo(); // s_joystick
            }

            Debug.Log("Player Count: " + PhotonNetwork.CountOfPlayers);

            //start spawning snowballs for player2
            player2Coroutine = StartCoroutine(Spawner(spawnPoints[1].position));
        }

    }

    void CreatePlayerOne(int level)
    {
        // Teleport + Snowman
        if ((level ==1) || (level == 0))
        {
            myPlayer1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "s_Teleport"),
                                                spawnPoints[0].position, spawnPoints[0].rotation);

            //set teleport-area
            groundPlayer1.GetComponent<TeleportationArea>().teleportationProvider = myPlayer1.GetComponent<TeleportationProvider>();
        }
        //Joystick + Snowman
        else if (level == 2)
        {
            myPlayer1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "s_Joystick"),
                                                spawnPoints[0].position, spawnPoints[0].rotation);
        }
        //Teleport + humanoid-avatar
        else if (level == 3)
        {
            myPlayer1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "h_Teleport"),
                                                 spawnPoints[0].position, spawnPoints[0].rotation);

            //set teleport-area
            groundPlayer1.GetComponent<TeleportationArea>().teleportationProvider = myPlayer1.GetComponent<TeleportationProvider>();
        }
        //Joystick + humanoid-avatar
        else if (level == 4)
        {
            myPlayer1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "h_Joystick"), spawnPoints[0].position, spawnPoints[0].rotation);
        }

    }

    // player two always stays the same (except in tutorial)
    void CreatePlayerTwo()
    {
        myPlayer2 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player2_s_Joystick"),
            spawnPoints[1].position, spawnPoints[1].rotation);
    }

    void CreatePlayerTwoTutorial()
    {
        myPlayer2 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player2_s_Teleport"),
            spawnPoints[1].position, spawnPoints[1].rotation);

        //set teleport-area
        groundPlayer2.GetComponent<TeleportationArea>().teleportationProvider = myPlayer2.GetComponent<TeleportationProvider>();
    }



    // Update is called once per frame
    void Update()
    {
        //update snowball-Counter
        if (snowballsSpawned >= maxSnowballs)
        {
            pauseCoroutine = true;
        }
        else
        {
            pauseCoroutine = false;
        }

        //would be better if only master handles time for all players
        //this should still work well enough because both players join roughly at the same time (<1sec)
        if (matchLength < 0f && !gameEnded)//end game when countdown reaches 0
        {
            Debug.Log("countdown 0 - try end game");
            photonView.RPC("StopSnowballs", RpcTarget.All); //stops coroutines for all players
            
            //TODO save statistics (playerCounts, movementWalked, numberOfTeleports etc.)
            //TODO: reset all variales via RPC 
            //TODO: test if menu needs reset too 
            gameEnded = true;

            // change back to menu scene (waiting room)
            photonView.RPC("BackToMenu", RpcTarget.MasterClient);
        }
        else if (matchLength >= 0f) //update countdown for "matchlength"-seconds
        {
            matchLength = matchLength - Time.deltaTime;
            timerText.SetText("" + (int)matchLength);
        }
        
    }
    
    //spawns snowballs around a given point (Vec3) while maxSnowballs < snowballsSpawned 
    IEnumerator Spawner(Vector3 spawnPoint)
    {
        yield return new WaitForSeconds(startWait);
        
        while (!pauseCoroutine)
        {
            //spawn in random position in general spawn-area of players
            Vector3 spawnPosition = new Vector3(Random.Range(spawnPoint.x - xArea, spawnPoint.x + xArea), 
                                                1, 
                                                Random.Range(spawnPoint.z - zArea, spawnPoint.z + zArea));

            Instantiate(snowball, spawnPosition + transform.TransformPoint(0, 0, 0), Quaternion.identity);
            snowballsSpawned++;
            Debug.Log("Snowball-counter:" + snowballsSpawned);
            //PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", "Snowball"), spawnPosition, spawnPoints[1].rotation);

            yield return new WaitForSeconds(spawnWait);
            
        }
    }

    [PunRPC]
    void StopSnowballs()
    {
        //stop spawning snowballs for master
        if (PhotonNetwork.IsMasterClient)
        {
            StopCoroutine(player1Coroutine);
        }
        else //stop spawning snowballs for !master
        {
            StopCoroutine(player2Coroutine);
        }

        //delete all snowballs
        foreach (GameObject sb in GameObject.FindGameObjectsWithTag("Snowball"))
        {
            Destroy(sb, 0.1f);
        }
    }

    [PunRPC]
    void UpdatePlayer1Score()
    {
        player1Score++;
        Debug.Log("player 1 scored");
        player1Text.SetText("" + player1Score);
        Debug.Log("Player1Panel changed");
    }

    [PunRPC]
    void UpdatePlayer2Score()
    {
        player2Score++;
        Debug.Log("player 2 scored");
        player2Text.SetText("" + player2Score);
        Debug.Log("Player2Panel changed");
    }


    [PunRPC]
    void BackToMenu()
    {
        //should/can only be called by master
        Debug.Log("try: back to menu");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    /// !!! currently not used !!!
    //because network obj. don't work with XR-interactable/grabbing
    //[PunRPC]
    //public void DeleteSnowball(PhotonView pv)
    //{
    //    //should/can only be called by master -> destroys obj instanciated by network
    //    Debug.Log("GSC: try destroying Snowball");
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.Destroy(pv);
    //    }
    //    Debug.Log("GSC: destroyed Snowball");
    //}
   
}
