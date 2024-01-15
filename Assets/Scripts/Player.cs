using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public bool normalEnemyMovement = true;
    [SerializeField] private float _playerRotateSpeed = 20.0f;

    //private FadeEffect _fadeEffect;
    private int _score;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private CameraShaker _camera;
    [SerializeField] private AudioManager _audioManager;

    [SerializeField] private bool _isPlayerInPosition = false;
    [SerializeField] private bool _isAsteroidDestroyed = false;
    [SerializeField] private bool _hasPlayerLaserCooledDown = false;
    [SerializeField] private bool _isPlayerMultiShotActive = false;
    [SerializeField] private bool _isPlayerLateralLaserActive = false;

    [Header("First Start / New Game Variables")]
    [SerializeField] private int _ammoCount = 20;
    [SerializeField] private int _maxAmmoStores = 25;
    [SerializeField] private int _homingMissileCount = 20;
    [SerializeField] private int _maxMissileStores = 25;
    [SerializeField] private bool _gameFirstStart = true;
    public int newGamePowerUpCollected = 0;

    [Header("Player Laser Variables")]
    [SerializeField] private float _wpnCoolDown;
    [SerializeField] private float _wpnReadyToFire;
    [SerializeField] private float _playerRateOfFire = 0.15f;

    [Header("Homing Missile Variables")]
    [SerializeField] private GameObject _playerHomingMissilePrefab;
    [SerializeField] private bool _isPlayerHomingMissilesActivate = false;

    [Header("Multi Shot Variables")]
    [SerializeField] private int _numberOfProjectiles = 3;
    [Range(0, 360)][SerializeField] private float _spreadAngle = 30;

    [Header("Speed Variables")]
    [SerializeField] private float _playerSpeed = 5.0f;
    [SerializeField] private float _speedMultiplier = 1.75f;
    [SerializeField] private bool _isPlayerSpeedBoostActive = false;

    [Header("Shields / Damage Variables")]
    [SerializeField] private bool _isPlayerShieldsActive = false;
    [SerializeField] private int _shieldHits = 0;
    [SerializeField] private float _playerShieldAlpha = 1.0f;
    private readonly GameObject[] _playerDamage;
    [SerializeField] private List<GameObject> poolDamageAnimations = new List<GameObject>();
    [SerializeField] private List<GameObject> activatedDamageAnimations = new List<GameObject>();

    [Header("Thruster Core Variables")]
    [SerializeField] private bool _coreOnline = true;
    [SerializeField] private int _coreTempDecrease;
    [SerializeField] private bool _coreTempCooledDown = true;
    [SerializeField] private bool _hasPlayerThrustersCooledDown = true;
    public ThrustersCoreTemp thrustersCoreTemp;
    public int maxCoreTemp = 1000;
    public int currentCoreTemp = 0;
    public bool canPlayerUseThrusters = false;
    public bool resetExceededCoreTempWarning = false;

    [Header("Tractor Beam Variables")]
    [SerializeField] private GameObject _tractorBeam;
    [SerializeField] private bool _canPlayerUseTractorBeam;
    [SerializeField] private bool _isTractorBeamOn = false;
    public TractorBeam tractorBeam;
    public int minTractorBeam = 0;
    public int maxTractorBeam = 1000;
    public int currentTractorBeam = 1000;
    [SerializeField] private float _scaleChangeRate;
    private Vector3 _scaleChange; // scale of Tractor Beam

    [Header("Inspector assigned")]
    [SerializeField] private int _playerLives = 3;
    public bool isPlayerAlive = true;

    [Header("UI Elements")]
    [SerializeField] private int _playerScore = 0;

    [Header("Game Objects")]
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _laserNode1; 
    [SerializeField] private GameObject _playerLaserPrefab;  
    [SerializeField] private GameObject _playerDoubleShotLaserPrefab;
    [SerializeField] private GameObject _playerMultiShotLaserPrefab;
    [SerializeField] private GameObject _playerLateralLaserPrefab;

    [SerializeField] private GameObject _playerShield;
    [SerializeField] private GameObject _playerThrusterLeft, _playerThrusterRight;
    [SerializeField] private GameObject _playerNoseThrusterLeft, _playerNoseThrusterRight;
    [SerializeField] private GameObject _lateralLaserCanonLeft, _lateralLaserCanonRight;
    [SerializeField] private GameObject _bigExplosionPrefab;

    public Coroutine LateralLaserTimer;
    public GameObject playerLaserContainer;
    public GameObject enemyBoss;


    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();

        _scaleChangeRate = 0.25f;
        _scaleChange = new Vector3(4.0f, 4.0f, 4.0f);
        transform.position = new Vector3(0f, -7.0f, 0f); // sets start px of Player 

        _camera = GameObject.Find("MainCamera").GetComponent<CameraShaker>();
        //_fadeEffect = GameObject.Find("CanvasFader").GetComponent<FadeEffect>();
        _audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        currentTractorBeam = 1000;
        tractorBeam.SetTractorBeam(currentTractorBeam);
        _canPlayerUseTractorBeam = false;

        currentCoreTemp = 0;
        thrustersCoreTemp.SetCoreTemp(currentCoreTemp);
        canPlayerUseThrusters = false;

        
        _uiManager.SetMaxAmmoCount(_maxAmmoStores);
        _uiManager.SetMaxMissileCount(_maxMissileStores);

        _uiManager.UpdateAmmoCount(_ammoCount);
        _uiManager.UpdateHomingMissileCount(_homingMissileCount);
        


        thrustersCoreTemp = GameObject.Find("ThrustersCoreTempHealthBar").GetComponent<ThrustersCoreTemp>();

        if (_gameManager == null)
        {
            Debug.LogError("The Game Manager is null.");
        }

        if (_spawnManager == null)
        {
            Debug.LogError("The SpawnManager is null.");
        }

        if (_uiManager == null)
        {
            Debug.LogError("The UIManager is null.");
        }

        if (_camera == null)
        {
            Debug.LogError("The CameraShaker on the Main Camera is null.");
        }

        if (_audioManager == null)
        {
            Debug.Log("AudioManager is NULL!");
        }

        _gameManager.PlayMusic(1, 5.0f);
        //_fadeEffect.FadeIn();

        newGamePowerUpCollected = 0;
    }

    public void FadeOut() // Fades out the Scene to black
    {
        //_fadeEffect.FadeOut();
    }

    void Update()
    {
    // Moves the Player from off screen into start position at -3.7f on the Y-axis
        if (_isPlayerInPosition == false)
        {
            transform.Translate(1.25f * Time.deltaTime * Vector3.up);

            if (transform.position.y >= -3.7f)
            {
                transform.position = new Vector3(transform.position.x, -3.7f, 0);
                _isPlayerInPosition = true;
            }

            if (_isPlayerInPosition == true)
            {
                StartCoroutine(ResetPlayerPosition());
            }
        }

        if (_gameManager.continueToNextDifficultyLevel == true)
        {
            _spawnManager.AdvanceToNextLevel();
            _gameManager.continueToNextDifficultyLevel = false;
        }

        if (_gameFirstStart == false)
        {
            PlayerMovement(); // old script was called CalculateMovement();
            CalculateThrustersScale();
            ActivateTractorBeam();
            ReplenishTractorBeam(2);
            ThrusterCoreLogic();

            if (_coreOnline == false)
            {
                //StartCoroutine(LoseCore());
            }

            if (Input.GetKeyDown(KeyCode.Space) && _hasPlayerLaserCooledDown)
            {
                PlayerFiresLaser();
            }
        }

        
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_isPlayerHomingMissilesActivate == true && _homingMissileCount > 0)
            {
                FireHomingMissile();
            }
        }

        if (_homingMissileCount == 0)
        {
            _isPlayerHomingMissilesActivate = false;
        }
        

        /*
        if (Input.GetKeyDown(KeyCode.N)) // testing purposes only, causes waves to attack from top or 360 degrees (See SpawnManager script)
        {
            if (normalEnemyMovement == false)
            {
                normalEnemyMovement = true;
            }
            else
            {
                normalEnemyMovement = false;
            }
        }
        */
    }

    void PlayerMovement()
    {
        float playerHorizontalInput = Input.GetAxis("Horizontal");
        float playerVerticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey("z")) // Player rotate CCW
        {
            _playerNoseThrusterLeft.SetActive(true);
            Vector3 playerRotateLeft = new Vector3(0, 0, 15.0f);
            transform.Rotate(_playerRotateSpeed * Time.deltaTime * playerRotateLeft);
        }
        else
        {
            _playerNoseThrusterLeft.SetActive(false);
        }

        if (Input.GetKey("c")) // Player rotate CW
        {
            _playerNoseThrusterRight.SetActive(true);
            Vector3 playerRotateRight = new Vector3(0, 0, -15.0f);
            transform.Rotate(_playerRotateSpeed * Time.deltaTime * playerRotateRight);
        }
        else
        {
            _playerNoseThrusterRight.SetActive(false);
        }

        Vector3 playerMovement = new Vector3(playerHorizontalInput, playerVerticalInput, 0);
        transform.Translate(_playerSpeed * Time.deltaTime * playerMovement);

        // limits Player y-axis if Start Game Asteroid is still active
        if (_isAsteroidDestroyed == false)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.7f, -1.0f), 0);
        }

        if (_isAsteroidDestroyed == true)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.7f, 5.0f), 0);
        }

        // x-axis boundaries
        if (transform.position.x > 11.25f)
        {
            transform.position = new Vector3(-11.25f, transform.position.y, 0);
            //transform.position = new Vector3(6.25f, transform.position.y, 0);

        }
        else if (transform.position.x < -11.25f)
        {
            transform.position = new Vector3(11.25f, transform.position.y, 0);
            //transform.position = new Vector3(-6.25f, transform.position.y, 0);

        }


    }


    // Tractor Beam Logic
    void ActivateTractorBeam()
    {
        if (Input.GetKey(KeyCode.T)) // Turns on the Tractor Beam
        {
            if (_canPlayerUseTractorBeam == true && currentTractorBeam > minTractorBeam)
            {
                TractorBeamCollectPwrUps.IsPowrUpTractorBeamActive = true;
                _tractorBeam.SetActive(true);
                _isTractorBeamOn = true;
                PlayerTractorBeamActivate(3);
            }
        }

        if (Input.GetKeyUp(KeyCode.T)) // Turns off the Tractor Beam
        {
            TractorBeamCollectPwrUps.IsPowrUpTractorBeamActive = false;
            _tractorBeam.SetActive(false);
            _isTractorBeamOn = false;
        }
    }

    void PlayerTractorBeamActivate(int tractorBeamDecrease)
    {
        currentTractorBeam -= tractorBeamDecrease;
        tractorBeam.SetTractorBeam(currentTractorBeam);
        if (currentTractorBeam < minTractorBeam)
        {
            currentTractorBeam = minTractorBeam;

            TractorBeamCollectPwrUps.IsPowrUpTractorBeamActive = false;
            _tractorBeam.SetActive(false);
            _isTractorBeamOn = false;
        }


        if (_isTractorBeamOn == true)
        {
            if (_scaleChangeRate == 0)
            {
                _scaleChangeRate =+ 0.1f;
            }

            _tractorBeam.transform.localScale += _scaleChange * _scaleChangeRate;

            if (_tractorBeam.transform.localScale.y < 4.0f || _tractorBeam.transform.localScale.y > 40.0f)
            {
                _scaleChange = -_scaleChange;
            }
        }
    }

    void ReplenishTractorBeam(int tractorBeamIncrease) // replenish TB when T released
    {
        if (currentTractorBeam < maxTractorBeam)
        {
            currentTractorBeam += tractorBeamIncrease;
            tractorBeam.SetTractorBeam(currentTractorBeam);
        }
    }



    // Start Asteroid Logic
    public void AsteroidBlockingSensors()
    {
        _audioManager.PlayAudioClip(0); // plays Asteroid Blocking Sensors audio clip
        StartCoroutine(WeaponsFree());
    }

    IEnumerator WeaponsFree()
    {
        yield return new WaitForSeconds(4.5f);
        canPlayerUseThrusters = true;
        _canPlayerUseTractorBeam = true;
        _uiManager.WeaponsFreeMsg();
    }

    public void LaserIsWpnsFree() // This is called from the NewGamePowerUp class once the initial ammo
                                  // and homing missile powerups are collected at the start of the game.  
    {
        _hasPlayerLaserCooledDown = true;
        newGamePowerUpCollected = 0;
    }

    public void AsteroidDestroyed()
    {
        _isAsteroidDestroyed = true;
        StartCoroutine(WarningIncomingWave(3.0f));
    }

    IEnumerator WarningIncomingWave(float time)
    {
        yield return new WaitForSeconds(time);
        _audioManager.PlayAudioClip(1); // plays Incoming Attack Wave audio clip

        _spawnManager.StartSpawning();
    }

    // Player Thrusters Logic
    void CalculateThrustersScale()
    {
        if (Input.GetKey(KeyCode.LeftShift) && _hasPlayerThrustersCooledDown && canPlayerUseThrusters)
        {
            PlayerThrustersActivate(5);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            PlayerThrustersDeactivate();
        }
    }

    void ThrusterCoreLogic()
    {
        if (currentCoreTemp > 0 && _coreTempCooledDown == true && canPlayerUseThrusters == true)
        {
            currentCoreTemp -= _coreTempDecrease;
            thrustersCoreTemp.SetCoreTemp(currentCoreTemp);
        }

        if (currentCoreTemp > 650 && _coreTempCooledDown == true && canPlayerUseThrusters == true)
        {
            _uiManager.CoreTempWarning(true);

            if (resetExceededCoreTempWarning == false)
            {
                resetExceededCoreTempWarning = true;
                StartCoroutine(PlayWarningCoreTempCritical());
            }
        }
        else
        {
            _uiManager.CoreTempWarning(false);
        }

        if (currentCoreTemp >= 999 && _coreTempCooledDown == true && canPlayerUseThrusters == true)
        {
            StartCoroutine(PlayWarningCoreTempExceeded());
            _coreTempCooledDown = false;
            canPlayerUseThrusters = false;

            PlayerCoreTempExceededDrifting();

            _uiManager.CoreShutdown(true);
        }

        if (currentCoreTemp > 250 && _coreTempCooledDown == false)
        {
            currentCoreTemp -= _coreTempDecrease;
            thrustersCoreTemp.SetCoreTemp(currentCoreTemp);

            PlayerCoreTempExceededDrifting();
        }

        if (currentCoreTemp == 250 && _coreTempCooledDown == false && canPlayerUseThrusters == false)
        {
            _coreTempCooledDown = true;
            canPlayerUseThrusters = true;
            _uiManager.CoreShutdown(false);
            currentCoreTemp -= _coreTempDecrease;
            thrustersCoreTemp.SetCoreTemp(currentCoreTemp);

            _playerThrusterLeft.gameObject.SetActive(true);
            _playerThrusterRight.gameObject.SetActive(true);
            _playerSpeed = 5.0f;

            _uiManager.CoreTempStable(true);

            if (transform.rotation.z != 0)
            {
                StartCoroutine(AnimateRotationTowards(this.transform, Quaternion.identity, 1f));
            }
        }
    }

    IEnumerator PlayWarningCoreTempCritical()
    {
        _audioManager.PlayAudioClip(3); // plays Core Temp Critical audio clip

        yield return new WaitForSeconds(3.0f);
        resetExceededCoreTempWarning = false;
    }

    IEnumerator PlayWarningCoreTempExceeded()
    {
        _audioManager.StopDialogueAudio();
        _audioManager.PlayAudioClip(4); // plays Core Temp Exceeded audio clip

        yield return new WaitForSeconds(5.0f);
    }

    /*
    //start it wherever you decide to start the animation. On key press, on trigger enter, on whatever.
    //in this example I'm rotating 'this', towards (0,0,0), for 1 second
    StartCoroutine(AnimateRotationTowards(this.transform, Quaternion.identity, 1f));
    */

    IEnumerator AnimateRotationTowards(Transform target, Quaternion rot, float dur)
    {
        _audioManager.PlayAudioClip(5); // plays Core Temp Nominal audio clip


        float t = 0f;
        Quaternion start = target.rotation;
        while (t < dur)
        {
            target.rotation = Quaternion.Slerp(start, rot, t / dur);
            yield return null;
            t += Time.deltaTime;
        }
        target.rotation = rot;

        _hasPlayerLaserCooledDown = true;
        _uiManager.CoreTempStable(false);
    }

    void PlayerCoreTempExceededDrifting()
    {
        _playerThrusterLeft.gameObject.SetActive(false);
        _playerThrusterRight.gameObject.SetActive(false);
        _hasPlayerLaserCooledDown = false;
        _playerSpeed = 0.25f;
        transform.Rotate(-50f * Time.deltaTime * Vector3.forward);
    }

    void PlayerThrustersActivate(int coreTempIncrease)
    {
        
        currentCoreTemp += coreTempIncrease;
        thrustersCoreTemp.SetCoreTemp(currentCoreTemp);
        if (currentCoreTemp > maxCoreTemp)
        {
            currentCoreTemp = maxCoreTemp;
        }

        _playerSpeed = 10.0f;
    }

    void PlayerThrustersDeactivate()
    {
        _playerSpeed = 5.0f;

        if (_isPlayerSpeedBoostActive == true)
        {
            _playerSpeed *= _speedMultiplier;
        }
    }

    // Player Fire Laser Logic
    void PlayerFiresLaser()
    {
        if (_hasPlayerLaserCooledDown == true && _ammoCount > 0)
        {
            _ammoCount -= 1;
            UpdatePlayerAmmoStores();


            if (_isPlayerMultiShotActive == true)
            {
                _wpnCoolDown = 1.0f;
                _wpnReadyToFire = Time.time + _wpnCoolDown;
                _playerRateOfFire = _wpnCoolDown;

                float angleStep = _spreadAngle / _numberOfProjectiles;
                float centeringOffset = (_spreadAngle / 2) - (angleStep / 2);
                float playerRotationOffset = transform.eulerAngles.z;

                for (int i = 0; i < _numberOfProjectiles; i++)
                {
                    float currentBulletAngle = (angleStep * i) + playerRotationOffset;
                    Vector3 laserPx = _laserNode1.transform.position;
                    Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, currentBulletAngle - centeringOffset));
                    GameObject playerLaser = Instantiate(_playerMultiShotLaserPrefab, laserPx, rotation);

                    playerLaser.transform.parent = _spawnManager.PlayerLaserContainer.transform;

                    _hasPlayerLaserCooledDown = false;
                    StartCoroutine(PlayerLaserCoolDownTimer());
                }
            }

            else if (_isPlayerMultiShotActive == false)
            {
                _wpnCoolDown = 0.5f;
                _wpnReadyToFire = Time.time + _wpnCoolDown;
                _playerRateOfFire = _wpnCoolDown;

                Vector3 laserPx = _laserNode1.transform.position;
                GameObject playerLaser = Instantiate(_playerDoubleShotLaserPrefab, laserPx, transform.rotation);

                playerLaser.transform.parent = _spawnManager.PlayerLaserContainer.transform;

                _hasPlayerLaserCooledDown = false;
                StartCoroutine(PlayerLaserCoolDownTimer());
            }

            if (_isPlayerLateralLaserActive == true)
            {
                Quaternion rotationLeft = Quaternion.Euler(new Vector3(0, 0, 90));
                Quaternion rotationRight = Quaternion.Euler(new Vector3(0, 0, 270));
                Instantiate(_playerLateralLaserPrefab, new Vector3(transform.position.x - 0.576f, transform.position.y + 0.054f, transform.position.z), rotationLeft);
                Instantiate(_playerLateralLaserPrefab, new Vector3(transform.position.x + 0.576f, transform.position.y + 0.054f, transform.position.z), rotationRight);
            }

            _audioManager.PlayAudioClip(2); // plays Laser audio clip

        }
    }

    // -----------------------------------------------------------------------------
    // Name	:	Player Laser Cool Down - Coroutine
    // Desc	:	Called when the Player presses fire to set the 
    //			_hasPlayerLasedCooledDownbool to false for duration of rate of fire.
    //			This stops the Player having a rapid fire capability
    // -----------------------------------------------------------------------------
    IEnumerator PlayerLaserCoolDownTimer()
    {
        yield return new WaitForSeconds(_playerRateOfFire);
        _hasPlayerLaserCooledDown = true;
    }



    private void FireHomingMissile()
    {
        Vector3 laserPx = _laserNode1.transform.position;
        GameObject playerMissile = Instantiate(_playerHomingMissilePrefab, laserPx, transform.rotation);

        playerMissile.transform.parent = _spawnManager.PlayerLaserContainer.transform;

        _homingMissileCount--;
        _uiManager.UpdateHomingMissileCount(_homingMissileCount);
    }



    // Player Damage Logic
    public void Damage()
    {
        if (_isPlayerShieldsActive == true)
        {
            _shieldHits++;

            switch (_shieldHits)
            {
                case 1:
                    _playerShieldAlpha = 0.75f;
                    _playerShield.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, _playerShieldAlpha);
                    _audioManager.StopDialogueAudio();
                    _audioManager.PlayAudioClip(6); // play Shield 65% audio clip
                    break;
                case 2:
                    _playerShieldAlpha = 0.40f;
                    _playerShield.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, _playerShieldAlpha);
                    _audioManager.StopDialogueAudio();
                    _audioManager.PlayAudioClip(7); // plays Shield 35% audio clip
                    break;
                case 3:
                    _isPlayerShieldsActive = false;
                    _playerShield.SetActive(false);
                    _audioManager.StopDialogueAudio();
                    _audioManager.PlayAudioClip(8); // play Shield Depleted audio clip
                    break;
            }
            return;
        }

        // randomly selects damaged area and sets it active.  The removes from pool to "Active" list.
        if (poolDamageAnimations.Count > 0)
        {
            var rdmDamage = Random.Range(0, poolDamageAnimations.Count);
            var temp = poolDamageAnimations[rdmDamage];
            activatedDamageAnimations.Add(temp);
            temp.SetActive(true);
            poolDamageAnimations.Remove(temp);
            _camera.StartDamageCameraShake(0.2f, 0.15f);
            return;
        }

        if (poolDamageAnimations.Count == 0)
        {
            _playerLives--;
            _uiManager.UpdateLives(_playerLives);
            _gameManager.UpdateLivesRemaining(_playerLives);
            _camera.StartDamageCameraShake(0.2f, 0.35f);
            Instantiate(_bigExplosionPrefab, transform.position, Quaternion.identity);
            _audioManager.PlayAudioClip(9); // plays explosion sound effect



            //if (_gameManager.lives != 0)
            if (_playerLives != 0)
            {
                //ResetDamageAnimationList();
                ResetStateOfCore();
             //   StartCoroutine(DisperseHomingMissiles());

                _isPlayerLateralLaserActive = false;
                _lateralLaserCanonLeft.SetActive(false);
                _lateralLaserCanonRight.SetActive(false);

                StartCoroutine(ResetPlayerPosition());
            }

            /*
            {
                ResetDamageAnimationList();
                _gameManager.GameOver();
                isPlayerAlive = false;
                _player.SetActive(false);
            }
            */
        }

        //if (_gameManager.lives < 1)
        if (_playerLives < 1)
        {
            ResetStateOfCore();
            _spawnManager.OnPlayerDeath();
            Instantiate(_bigExplosionPrefab, transform.position, Quaternion.identity);
            _player.SetActive(false);
            isPlayerAlive = false;

            //Destroy(this.gameObject);
        }
    }

    IEnumerator ResetPlayerPosition()
    {
        if (_gameFirstStart == false)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        GetComponent<BoxCollider2D>().enabled = false; 

        _playerSpeed = 0;
        _hasPlayerLaserCooledDown = false;
        canPlayerUseThrusters = false;
        _canPlayerUseTractorBeam = false;
        _gameManager.isPlayerDestroyed = true;
        ResetDamageAnimationList();

        _uiManager.ReadySetGo();
        yield return new WaitForSeconds(0.8f);
        transform.position = new Vector3(0, -3.7f, 0); // Player returns to default start position
        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(3.2f);

        if (_isAsteroidDestroyed == true)
        {
            _hasPlayerLaserCooledDown = true;
            canPlayerUseThrusters = true;
            _canPlayerUseTractorBeam = true;
        }

        GetComponent<BoxCollider2D>().enabled = true;


        _playerSpeed = 5.0f;

        if (_gameFirstStart == true && _isAsteroidDestroyed == false)
        {
            _gameFirstStart = false;
            _spawnManager.OnPlayerReady();
        }

        if (_isAsteroidDestroyed == true)
        {
            _spawnManager.OnPlayerReady();
            _spawnManager.StartSpawning();
        }
    }

    public void ResetDamageAnimationList()
    {
     //   _spawnManager.OnPlayerReset();

        while (activatedDamageAnimations.Count > 0)
        {
            var rdmDamage = Random.Range(0, activatedDamageAnimations.Count);
            var temp = activatedDamageAnimations[rdmDamage];
            poolDamageAnimations.Add(temp);
            temp.SetActive(false);
            activatedDamageAnimations.Remove(temp);
        }
    }

    public void ResetStateOfCore()
    {
        _audioManager.StopDialogueAudio();

        transform.rotation = Quaternion.identity;
        _uiManager.CoreTempStable(false);
        _uiManager.CoreTempWarning(false);
        _uiManager.CoreShutdown(false);
        _coreTempCooledDown = true;
        currentCoreTemp = 0;
        thrustersCoreTemp.SetCoreTemp(currentCoreTemp);
        _playerThrusterLeft.gameObject.SetActive(true);
        _playerThrusterRight.gameObject.SetActive(true);
    }

    public void AddScore(int points)
    {
        _playerScore += points;
        _uiManager.UpdateScore(_playerScore);
    }


    // ----------------------------------------------------------------------------
    // Name	:	PowerUp Collected
    // Desc	:	Called when the Player collects a Power Up
    // ----------------------------------------------------------------------------
    public void MultiShotActivate()
    {
        _isPlayerMultiShotActive = true;
        StartCoroutine(MultiShotCoolDownTimer());
    }

    IEnumerator MultiShotCoolDownTimer()
    {
        yield return new WaitForSeconds(5.0f);
        _isPlayerMultiShotActive = false;
    }

    public void SpeedBoostActivate()
    {
        if (_isPlayerSpeedBoostActive == false) // only give the Player a temp speed boost if the PowerUp is not already collected
        {
            _isPlayerSpeedBoostActive = true;
            _playerSpeed *= _speedMultiplier;
            StartCoroutine(SpeedBoostPowerDownTimer());
        }
        else
        {
            return;
        }
    }

    IEnumerator SpeedBoostPowerDownTimer()
    {
        yield return new WaitForSeconds(5.0f);
        _playerSpeed /= _speedMultiplier;
        _isPlayerSpeedBoostActive = false;
    }

    public void ShieldsActivate()
    {
        _isPlayerShieldsActive = true;
        _playerShield.SetActive(true);
        _shieldHits = 0;
        _playerShieldAlpha = 1.0f;
        _playerShield.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, _playerShieldAlpha);
    }

    public void PlayerRegularAmmo(int ammoCollected)
    {
        _ammoCount += ammoCollected;
        UpdatePlayerAmmoStores();
    }

    public void UpdatePlayerAmmoStores()
    {
        if (_ammoCount < 0)
        {
            _ammoCount = 0;
        }
        else if (_ammoCount > _maxAmmoStores)
        {
            _ammoCount = _maxAmmoStores;
        }

        _uiManager.SetMaxAmmoCount(_maxAmmoStores);
        _uiManager.UpdateAmmoCount(_ammoCount);
    }

    public void PlayerHomingMissiles(int homingMissilesCollected)
    {
        _homingMissileCount += homingMissilesCollected;
        _isPlayerHomingMissilesActivate = true;
        UpdatePlayerMissileStores();
    }

    public void UpdatePlayerMissileStores()
    {
        if (_homingMissileCount < 0)
        {
            _homingMissileCount = 0;
        }
        else if (_homingMissileCount > _maxMissileStores)
        {
            _homingMissileCount = _maxMissileStores;
        }

        _uiManager.SetMaxMissileCount(_maxMissileStores);
        _uiManager.UpdateHomingMissileCount(_homingMissileCount);
    }

        public void LateralLaserShotActive()
    {
        if (_isPlayerLateralLaserActive == false) // equips Player with lateral laser canons and starts the power down timer.
        {
            _isPlayerLateralLaserActive = true;
            _lateralLaserCanonLeft.SetActive(true);
            _lateralLaserCanonRight.SetActive(true);
            StartLateralLaserTimerCoroutine();
            return;
        }

        if (_isPlayerLateralLaserActive == true) // drops old canons if equipped, loads new ones, and resets the power down timer.
        {
            StopLateralLaserTimerCoroutine();
            DropLateralLaserCanons();
            _isPlayerLateralLaserActive = true;
            _lateralLaserCanonLeft.SetActive(true);
            _lateralLaserCanonRight.SetActive(true);
            StartLateralLaserTimerCoroutine();
        }
    }

    public void StartLateralLaserTimerCoroutine() // starts the coroutine to power down lateral laser canons after 15 seconds
    {
        LateralLaserTimer = StartCoroutine(LateralShotPowerDownTimer());
    }

    public void StopLateralLaserTimerCoroutine() // stops the coroutine to power down lateral laser canons (resets timer)
    {
        StopCoroutine(LateralLaserTimer);
    }

    IEnumerator LateralShotPowerDownTimer()
    {
        yield return new WaitForSeconds(15.0f);
        DropLateralLaserCanons();
    }

    public void DropLateralLaserCanons()
    {
        _isPlayerLateralLaserActive = false;
        _lateralLaserCanonLeft.SetActive(false);
        _lateralLaserCanonRight.SetActive(false);

        Instantiate(_spawnManager.DepletedLateralLaserCanons, transform.position, Quaternion.identity);

        transform.Translate(_gameManager.currentPowerUpSpeed * Time.deltaTime * Vector3.down);

        if (transform.position.y < -9.0f)
        {
            Destroy(this.gameObject);
        }
    }

    public void HealthBoostActivate()
    {
        // Reverses damage by removing random (if more than 1 active) damages area and returning it to the pool.
        if (activatedDamageAnimations.Count > 0)
        {
            var rdmDamage = Random.Range(0, activatedDamageAnimations.Count);
            var temp = activatedDamageAnimations[rdmDamage];
            poolDamageAnimations.Add(temp);
            temp.SetActive(false);
            activatedDamageAnimations.Remove(temp);
        }
    }

    public void NegativePowerUpCollision()
    {
        int _itemLost = Random.Range(0, 4);

        switch (_itemLost)
        {
            case 0:
                LoseAmmo();
                break;
            case 1:
                LoseMissiles();
                break;
            case 2:
                _coreOnline = false;
                StartCoroutine(LoseCore());
                break;
            case 3:
                LoseShields();
                break;
        }
    }

    void LoseAmmo()
    {
        _ammoCount -= (Random.Range(5, 10));

        if (_ammoCount < 0)
        {
            _ammoCount = 0;
        }
        _uiManager.UpdateAmmoCount(_ammoCount);
    }

    void LoseMissiles()
    {
        _homingMissileCount -= (Random.Range(1, 10));
        if (_homingMissileCount < 0)
        {
            _homingMissileCount = 0;
            _isPlayerHomingMissilesActivate = false;
        }
        _uiManager.UpdateHomingMissileCount(_homingMissileCount);
    }

    IEnumerator LoseCore()
    {
        transform.Rotate(Vector3.forward * -50f * Time.deltaTime);
        _hasPlayerLaserCooledDown = false;
        canPlayerUseThrusters = false;
        _playerThrusterLeft.gameObject.SetActive(false);
        _playerThrusterRight.gameObject.SetActive(false);
        _playerSpeed = 0.15f;

        //yield return new WaitForSeconds(5.0f);
        yield return new WaitForSeconds(Random.Range(3f, 6f));

        canPlayerUseThrusters = true;
        _playerThrusterLeft.gameObject.SetActive(true);
        _playerThrusterRight.gameObject.SetActive(true);
        _playerSpeed = 5.0f;

        if (transform.rotation.z != 0)
        {
            StartCoroutine(RotatePlayerUp(this.transform, Quaternion.identity, 1f));
        }

        _coreOnline = true;
    }

    void LoseShields()
    {
        _isPlayerShieldsActive = false;
        _audioManager.PlayAudioClip(8);
        _playerShield.SetActive(false);
    }

    IEnumerator RotatePlayerUp(Transform target, Quaternion rot, float dur)
    {
        float t = 0f;
        Quaternion start = target.rotation;
        while (t < dur)
        {
            target.rotation = Quaternion.Slerp(start, rot, t / dur);
            yield return null;
            t += Time.deltaTime;
        }
        target.rotation = rot;

        _hasPlayerLaserCooledDown = true;
    }

}
