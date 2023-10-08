using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

public class Main : MonoBehaviour {
    static private Main S;                        // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;
    public GameObject[] GOUI;
    public AudioSource music;
    public AudioSource gameOver;

    /*Audio Sources: 
    * https://freesound.org/people/Romariogrande/sounds/396231/
    * https://freesound.org/people/MATRIXXX_/sounds/414888/
    * https://freesound.org/people/MATRIXXX_/sounds/515005/
    */

    [Header("Inscribed")]
    public bool          spawnEnemies = true;
    public GameObject[] prefabEnemies;               // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f;  // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f;    // Inset from the sides
    public float gameRestartDelay = 2;
    public GameObject           prefabPowerUp;
    public WeaponDefinition[]   weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {
        eWeaponType.blaster, eWeaponType.blaster,
        eWeaponType.spread, eWeaponType.shield
    };
    


    private BoundsCheck bndCheck;

    void Awake() {
        S = this;

        GOUI = GameObject.FindGameObjectsWithTag("GOUI"); //Turn off the Game Over UI
        foreach (GameObject G in GOUI){
            G.SetActive(false);
        }

        // Set bndCheck to reference the BoundsCheck component on this 
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnPerSecond );
        // A generic Dictionary with eWeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();  
        foreach (WeaponDefinition def in weaponDefinitions) {     
            WEAP_DICT[def.type] = def;
        }

    }

    public void SpawnEnemy() {
        // If spawnEnemies is false, skip to the next invoke of SpawnEnemy()
        if (!spawnEnemies) {
            Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);
            return;
        }
        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;
        if (go.GetComponent<BoundsCheck>() != null) {
            enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned Enemy
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), 1f/enemySpawnPerSecond);
    }

    static public void Restart() {
        //
        // Reload __Scene_0 to restart the game
        // "__Scene_0" below starts with 2 underscores and ends with a zero.
        SceneManager.LoadScene("Scene_0");                               
    }

    static public void Exit(){
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    void DelayedRestart() {                                                   
    // Invoke the Restart() method in gameRestartDelay seconds
    music.Pause();
    gameOver.Play(0);
    Invoke(nameof(methodGOUI), gameRestartDelay );                    
    }

    void methodGOUI(){
        foreach (GameObject G in GOUI){
                G.SetActive(true);
        } 
    }

   

    static public void HERO_DIED() {
        S.DelayedRestart();                                                  
    }

    /// <summary>
    /// Static function that gets a WeaponDefinition from the WEAP_DICT static
    /// protected field of the Main class.
    /// </summary>
    /// <returns>The WeaponDefinition, or if there is no WeaponDefinition with
    ///   the eWeaponType passed in, returns a new WeaponDefinition with a
    ///   eWeaponType of eWeaponType.none.</returns>
    /// <param name="wt">The eWeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt) {  // a
        if (WEAP_DICT.ContainsKey(wt)) {                                      // b
            return (WEAP_DICT[wt]);
        }
        // If no entry of the correct type exists in WEAP_DICT, return a new
        //   WeaponDefinition with a type of eWeaponType.none (the default value)
        return (new WeaponDefinition());                                     // c
    }

    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes creates
    /// a PowerUp in place of the destroyed ship.
    /// </summary>
    /// <param name="e">The Enemy that was destroyed</param>
    static public void SHIP_DESTROYED( Enemy e ) {
        // Potentially generate a PowerUp
        if (Random.value <= e.powerUpDropChance) {
            // Choose a PowerUp from the possibilities in powerUpFrequency
            int ndx = Random.Range( 0, S.powerUpFrequency.Length );
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            // Spawn a PowerUp
            GameObject go = Instantiate<GameObject>( S.prefabPowerUp );
            PowerUp pUp = go.GetComponent<PowerUp>();
            // Set it to the proper WeaponType
            pUp.SetType( pUpType );

            // Set it to the position of the destroyed ship
            pUp.transform.position = e.transform.position;
        }
    }
}

