using UnityEngine;

public class Gun : MonoBehaviour
{
    private ProjectileMode mode;
    private FireMode firemode;
    private float speed = 25f;

    public Camera FpsCam;
    public GameObject Cross;
    public GameObject Projectile;
    public GameObject Startpoint;
    public Input.ShootingActions Input;

    private void Awake()
    {
        Input = new Input().Shooting;
        Input.Enable();
    }

    void Start()
    {
        mode = ProjectileMode.Positive;
        firemode = FireMode.Direct;
        var direction = Cross.transform.position - FpsCam.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.Shoot.WasPerformedThisFrame())
        {
            Instantiate(Projectile, Startpoint.transform.position, Quaternion.identity);
        }

        if (Input.ProjectileModeSwitch.WasPerformedThisFrame())
        { Toggle(); }

        if (Input.FireModeSwitch.WasPerformedThisFrame())
        {
            /*if (firemode == FireMode.Direct)
            { firemode = FireMode.Bounce; }
            else
            { firemode = FireMode.Direct;} */ 
            // wurde durch Zeile darunter ersetzt (erfüllt dieselbe Aufgabe)

            firemode = firemode == FireMode.Direct ? FireMode.Bounce : FireMode.Direct;
            //ternary operator (Abfrage ist  X == YZ? "Wenn ja, mach das hier" : "Wenn nein, mach das hier"
        }
    }


    public void Toggle()
    { mode = (ProjectileMode)((int)mode * -1); }

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
