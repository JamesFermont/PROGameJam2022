using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform fpsCam;
    [SerializeField] private Projectile projectile;
    [SerializeField] private float fireRateSeconds = 1f;
    [SerializeField, Min(0.1f)] private float aimDownSightDuration, stopAimDuration;

    private WeaponMove _weaponMove;
    private bool _isFiring;
    private float _timeToNextFire;
    private Input.ShootingActions _input;
    private ProjectileMode _mode;
    private FireMode _fireMode;

    private void Awake()
    {
        _input = new Input().Shooting;
        _input.Enable();

        _weaponMove ??= GetComponent<WeaponMove>();
    }

    private void Start()
    {
        _mode = ProjectileMode.Positive;
        _fireMode = FireMode.Direct;
    }

    // Update is called once per frame
    private void Update()
    {
        if ( _input.ProjectileModeSwitch.WasPerformedThisFrame() ) {
            Toggle();
            Debug.Log("Projectile Mode set to " + _mode);
        }

        if (_input.FireModeSwitch.WasPerformedThisFrame())
        {
            /*if (firemode == FireMode.Direct)
            { firemode = FireMode.Bounce; }
            else
            { firemode = FireMode.Direct;} */ 
            // wurde durch Zeile darunter ersetzt (erfüllt dieselbe Aufgabe)

            _fireMode = _fireMode == FireMode.Direct ? FireMode.Bounce : FireMode.Direct;
            Debug.Log("Fire Mode set to " + _fireMode);
            //ternary operator (Abfrage ist  X == YZ? "Wenn ja, mach das hier" : "Wenn nein, mach das hier"
        }

        if ( _input.Shoot.WasPerformedThisFrame() ) {
            _isFiring = true;
        } else if ( _input.Shoot.WasReleasedThisFrame() ) {
            _isFiring = false;
        }

        if ( Time.realtimeSinceStartup < _timeToNextFire ) {
            return;
        }

        if (_isFiring)
        {
            Debug.Log("Pew!");
            StartCoroutine(AimDownSight());
            _timeToNextFire = Time.realtimeSinceStartup + fireRateSeconds;
        }
    }

    private IEnumerator AimDownSight() {
        float timeElapsed = 0f;

        while (timeElapsed < aimDownSightDuration) {
            float t = timeElapsed / aimDownSightDuration;
            _weaponMove.SetLerpValue(t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        _weaponMove.SetLerpValue(1f);
        Projectile p = Instantiate(projectile, fpsCam.position + fpsCam.forward, fpsCam.rotation);
        p.mode = _mode;
        p.hitCount = (int)_fireMode;
        StartCoroutine(StopAim());
    }

    private IEnumerator StopAim() {
        float timeElapsed = 0f;
        
        while (timeElapsed < stopAimDuration) {
            float t = timeElapsed / stopAimDuration;
            _weaponMove.SetLerpValue(1f - t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        _weaponMove.SetLerpValue(0f);
    }

    private void Toggle() {
        _mode = (ProjectileMode)((int)_mode * -1);
    }

    public enum ProjectileMode
    {
        Positive = 1,
        Negative = -1
    }

    public enum FireMode
    {
        Direct = 1,
        Bounce = 2
    }
}
