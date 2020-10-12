using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameSetupController : MonoBehaviour
{
    public int level; //set in editor (used to spawn different player-prefabs in every lvl)
    public Transform[] spawnPoints = new Transform[2];
    public static GameSetupController GSC;
  
    int player1Score;
    int player2Score;
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public GameObject groundPlayer1;
    public GameObject groundPlayer2;
    public GameObject myPlayer1;
    public GameObject myPlayer2;
    public PhotonView photonView;
    public int snowballsSpawned;
    public int maxSnowballs;
    public int endOfGameSBThrownP1;
    public int endOfGameSBThrownP2;
    public GameObject snowball;
    Coroutine player1Coroutine;
    Coroutine player2Coroutine;
    float xArea; //Area for snowball-spawns
    float zArea;
    float spawnWait; //time between sb spawns
    float startWait; //time before sb start spawning
    public TextMeshProUGUI timerText;
    float matchLength; //in seconds
    bool gameEnded;

    public int numberOfTeleports;
    public int numberOfTeleportsP2;
    public float localMovement;
    public float MovementP1;
    public float MovementP2;
    private string playerType;
    private string playerTypeP2;


    private void Awake()
    {
        GSC = this;
        photonView = GetComponent<PhotonView>();

        maxSnowballs = 15;
        xArea = 13.0f; //Area for sb spawns (+/- xArea)
        zArea = 4.0f;
        spawnWait = 1.0f; //time between sb spawns 
        startWait = 0.0f; //time before sb start spawning
        
        if (level==101)
        {
            matchLength = 60; //90
        }
        else if (level == 102)
        {
            matchLength = 30; //30
        }
        else
        {
            matchLength = 120; //120sec default time
        }
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
            player1Coroutine = StartCoroutine(Spawner());
        }
        else //spawn at second spawn-point (for everyone else)
        {
            //creates PlayerAvatar ("PhotonPlayer") at a spawn-point 2
            Debug.Log("Creating Player2");

            if (level == 101)
            {
                CreatePlayerTwoTutorial(); // s_teleport
            }
            else
            {
                CreatePlayerTwo(); // s_joystick
            }

            Debug.Log("Player Count: " + PhotonNetwork.CountOfPlayers);

            //start spawning snowballs for player2
            player2Coroutine = StartCoroutine(Spawner());
        }

    }

    void CreatePlayerOne(int level)
    {
        // Teleport + Snowman
        if ((level ==1) || (level == 101))
        {
            myPlayer1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "s_Teleport"),
                                                spawnPoints[0].position, spawnPoints[0].rotation);

            //set teleport-area
            groundPlayer1.GetComponent<TeleportationArea>().teleportationProvider = myPlayer1.GetComponent<TeleportationProvider>();
        }
        //Joystick + Snowman
        else if ((level == 2) || (level == 102))
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
        // stop snowball spawns, delete snowballs, update statistics, save into txt and go back to menu
        if (matchLength < 0f && !gameEnded && PhotonNetwork.IsMasterClient)//end game when countdown reaches 0
        {
            gameEnded = true;
            Debug.Log("countdown 0 - try end game");
            
            StartCoroutine(EndGame());
        }
        else if (matchLength >= 0f) //update countdown for "matchlength"-seconds
        {
            matchLength = matchLength - Time.deltaTime;
            timerText.SetText("" + (int)matchLength);
        }
    }

    IEnumerator EndGame()
    {
        // photonView.RPC("StopSnowballs", RpcTarget.All); //stops coroutines for all players + deletes sb

        Debug.Log("end of game stats p1");
        MovementP1 = localMovement;
        if (numberOfTeleports == 0)
        {
            playerType = "Joystick";
        }
        else
        {
            playerType = "Teleport";
        }

        Debug.Log("end of game stats p2");
        photonView.RPC("UpdateAllStats", RpcTarget.Others);
        yield return new WaitForSeconds(1.0f);

        numberOfTeleportsP2 = (int)PhotonNetwork.MasterClient.CustomProperties["numberOfTeleportsP2"];
        MovementP2 = (float)PhotonNetwork.MasterClient.CustomProperties["MovementP2"];
        endOfGameSBThrownP2 = (int)PhotonNetwork.MasterClient.CustomProperties["endOfGameSBThrownP2"];

        if (numberOfTeleportsP2 == 0)
        {
            playerTypeP2 = "Joystick";
        }
        else
        {
            playerTypeP2 = "Teleport";
        }
        
        //save statistics (playerCounts, movementWalked, numberOfTeleports etc.)
        photonView.RPC("SaveStats", RpcTarget.MasterClient);

        Debug.Log("reset stats ");
        photonView.RPC("RPCresetStats", RpcTarget.All);
        yield return new WaitForSeconds(0.5f);

        // change back to menu scene (waiting room)
        photonView.RPC("BackToMenu", RpcTarget.MasterClient);
        
    }

    //spawns snowballs around a given point (Vec3) until certain amount is reached (maxSnowballs)
    IEnumerator Spawner()
    {
        //increase this time if you don't want the sb to spawn instantly after connecting
        yield return new WaitForSeconds(startWait);
        
        while (snowballsSpawned < maxSnowballs) //!pauseCoroutine 
        {
            SpawnSnowballAtRandomLoc();
            // Debug.Log("Snowball-counter:" + snowballsSpawned);

            yield return new WaitForSeconds(spawnWait);
        }
        Debug.Log("Snowball-counter:" + snowballsSpawned);
    }

    public void SpawnSnowballAtRandomLoc()
    {
        Vector3 spawnPointSB;

        if (PhotonNetwork.IsMasterClient)
        {
            spawnPointSB = spawnPoints[0].position;

            //spawn in random position in general spawn-area of players
            Vector3 spawnPosition = new Vector3(Random.Range(spawnPointSB.x - xArea, spawnPointSB.x + xArea),
                                                1,
                                                Random.Range(spawnPointSB.z - zArea, spawnPointSB.z + zArea));

            //Instantiate(snowball, spawnPosition + transform.TransformPoint(0, 0, 0), Quaternion.identity);
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Snowball"), spawnPosition + transform.TransformPoint(0, 0, 0), Quaternion.identity);
            snowballsSpawned++;
        }
        else //spawn at second spawn-point
        {
            spawnPointSB = spawnPoints[1].position;
            Vector3 spawnPosition = new Vector3(Random.Range(spawnPointSB.x - xArea, spawnPointSB.x + xArea),
                                                1,
                                                Random.Range(spawnPointSB.z - zArea, spawnPointSB.z + zArea));

            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "SnowballP2"), spawnPosition + transform.TransformPoint(0, 0, 0), Quaternion.identity);
            snowballsSpawned++;
        }
    }

    //triggered with button-Press (leftHand controller)
    // moved to CustomTeleportSetup
    //public void teleportCounter()
    //{
    //    localMovement++;
    //    MovementP1++;
    //    Debug.Log("localMovement (tp): " + localMovement);
    //    Debug.Log("MovementP1 (tp): " + MovementP1);
    //}
    
    /// PUN - RPCs --------------------------------------------------------
    
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

    //called on other/remote Player -> updates stats for master
    [PunRPC]
    void UpdateAllStats()
    {
       // endOfGameSBThrownP2 = snowballsSpawned - maxSnowballs;
       
        Hashtable hash = new Hashtable();
        hash.Add("MovementP2", localMovement);
        hash.Add("endOfGameSBThrownP2", endOfGameSBThrownP2);
        hash.Add("numberOfTeleportsP2", numberOfTeleports);
        PhotonNetwork.MasterClient.SetCustomProperties(hash);

    }

    [PunRPC]
    void RPCresetStats()
    {
        gameEnded = false;
        player1Score = 0;
        player2Score = 0;

        endOfGameSBThrownP1 = 0;
        endOfGameSBThrownP2 = 0;
        snowballsSpawned = 0;

        numberOfTeleports = 0;
        localMovement = 0f;
        MovementP1 = 0f;
        MovementP2 = 0f;

        Hashtable hash = new Hashtable();
        hash.Add("MovementP2", localMovement);
        hash.Add("endOfGameSBThrownP2", endOfGameSBThrownP2);
        PhotonNetwork.MasterClient.SetCustomProperties(hash);
        //todo test
        PhotonNetwork.MasterClient.CustomProperties["MovementP2"] = 0f;
        PhotonNetwork.MasterClient.CustomProperties["endOfGameSBThrownP2"] = 0;
        
    }

    //saves statistics for both players and saves it into txt-file (Asset-Folder)
    // RPC not necessary, currently only called on master once
    [PunRPC] 
    void SaveStats()
    {
        Debug.Log("saving stats");

        //path of the file
        string path = Application.dataPath + "/GameStatistics.txt";
        Debug.Log("path: " + path);
        
        //create if it doesn't exist, otherwise append
        if (!File.Exists(path))
        {
            Debug.Log("create File");
            File.WriteAllText(path, "StartContent \n How2Read: \n 101+102=tutorial-lvl, 1=tj, 2=jj, 3=H-tj, 4=H-jj \n --------------------------------------- \n");
        }
        string LevelAndTime = "Time: " + System.DateTime.Now + "\n" + "Stats for Level: " + level + "\n\n";
        string playerStatsP1 = "Player 1 (Master): player-type" + playerType + "\n total movement:" + MovementP1 + "\n number of teleports: " + numberOfTeleports + "\n SnowballsThrown: " + endOfGameSBThrownP1 + "\n player1Score: " + player1Score + "\n \n";
        string playerStatsP2 = "Player 2: player-type"          + playerTypeP2 + "\n total movement: " + MovementP2 + "\n number of teleports: " + numberOfTeleportsP2 + "\n SnowballsThrown: " + endOfGameSBThrownP2 + "\n player2Score: " + player2Score + "\n";
        string devider = "--------------------------------------- \n";

        //appends current stats to txt-file
        File.AppendAllText(path, LevelAndTime);
        File.AppendAllText(path, playerStatsP1);
        File.AppendAllText(path, playerStatsP2);
        File.AppendAllText(path, devider);
    }

    [PunRPC]
    void BackToMenu()
    {
        //should/can only be called by master
        Debug.Log("back to menu");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    //[PunRPC]
    //void StopSnowballs()
    //{
    //    //stop spawning snowballs for master
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        StopCoroutine(player1Coroutine);

    //        //delete all snowballs that you own/created and that are not currently hold 
    //        foreach (GameObject sb in GameObject.FindGameObjectsWithTag("Snowball"))
    //        {
    //            //Destroy(sb, 0.1f);

    //            //TODO still crashes for p2 - disabled for now ... sceneChange deletes everything anyways
    //            if (sb.GetPhotonView().IsMine && !sb.GetComponent<CollideScriptSnowball>().SnowballGrabbed)
    //            {
    //                // photonView.RPC("DeleteSnowball", RpcTarget.MasterClient, sb.GetPhotonView());
    //                Debug.Log("GSC: try destroying Snowball");
    //                PhotonNetwork.Destroy(sb);
    //            }
    //        }
    //    }
    //    else //stop spawning snowballs for !master
    //    {
    //        StopCoroutine(player2Coroutine);
    //        foreach (GameObject sb in GameObject.FindGameObjectsWithTag("SnowballP2"))
    //        {
    //            if (sb.GetPhotonView().IsMine && !sb.GetComponent<CollideScriptSnowball>().SnowballGrabbed)
    //            {
    //                Debug.Log("GSC: try destroying Snowball P2");
    //                PhotonNetwork.Destroy(sb);
    //            }
    //        }
    //    }
    //}

    // currently not used - strg+k strg+c --- strg+k strg+u
    //[PunRPC]
    //public void DeleteSnowball(PhotonView pv)
    //{
    //    //should/can only be called by master -> destroys obj instanciated by network
    //    Debug.Log("GSC: try destroying Snowball");
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //       pv.TransferOwnership(PhotonNetwork.MasterClient);
    //        PhotonNetwork.Destroy(pv);
    //    }
    //    Debug.Log("GSC: destroyed Snowball");
    //}

}
