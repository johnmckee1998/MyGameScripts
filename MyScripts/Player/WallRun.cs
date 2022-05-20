using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;


public class WallRun : MonoBehaviour
{
    public static WallRun instance;

    public float wallMaxDistance = 1;
    public float wallSpeedMultiplier = 1.2f;
    public float minimumHeight = 1.2f;
    public float maxAngleRoll = 20;
    [Range(0.0f, 1.0f)]
    public float normalizedAngleThreshold = 0.1f;
    
    public float jumpDuration = 1;
    public float wallBouncing = 3;
    public float cameraTransitionDuration = 1;

    public float wallGravityDownForce = 20f;

    public bool useSprint;


    [Space]
    public Volume wallRunVolume;

    //PlayerCharacterController m_PlayerCharacterController;
    //PlayerInputHandler m_InputHandler;

    Vector3[] directions;
    RaycastHit[] hits;

    bool isWallRunning = false;
    Vector3 lastWallPosition;
    Vector3 lastWallNormal;
    float elapsedTimeSinceJump = 0;
    float elapsedTimeSinceWallAttach = 0;
    float elapsedTimeSinceWallDetatch = 0;
    public bool jumping;
    float lastVolumeValue = 0;
    float noiseAmplitude;

    //bool isPlayergrounded() => m_PlayerCharacterController.isGrounded;
    private bool prevGrounded = false;
    public bool IsWallRunning() => isWallRunning;
    

    bool CanWallRun()
    {
        //float verticalAxis = Input.GetAxisRaw(GameConstants.k_AxisNameVertical); //vertical input?
        //bool isSprinting = m_InputHandler.GetSprintInputHeld(); //which key is held?
        //isSprinting = !useSprint ? true : isSprinting; //if sprinting - only used is useSprint is true

        // return !CharacterControllerScript.characterController.isGrounded && verticalAxis > 0 && VerticalCheck() && isSprinting; //based off all properties can you wall run
        bool isSprinting = true; //if sprinting - only used is useSprint is true
        if (useSprint)
            isSprinting = CharacterControllerScript.isSprinting;
        return (!CharacterControllerScript.characterController.isGrounded /*&& Input.GetKeyDown("w")*/ && VerticalCheck() && !CharacterControllerScript.instance.getCrouch() && (Input.GetAxisRaw("Fire3")!=0));
    }

    bool VerticalCheck()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumHeight);
    }


    void Start()
    {
        //if (instance == null)
            instance = this;
        //else
        //    Destroy(this);
        //m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
        //m_InputHandler = GetComponent<PlayerInputHandler>();

         directions = new Vector3[]{ 
            Vector3.right, 
            Vector3.right + Vector3.forward,
            Vector3.forward, 
            Vector3.left + Vector3.forward, 
            Vector3.left
        };

        if(wallRunVolume != null)
        {
            SetVolumeWeight(0);
        }
    }

    private void Update()
    {
        //if (Input.GetButton("Jump")) //improve
        //    jumping = true;
        //else
        //    jumping = false;

        CharacterControllerScript.instance.canWallRun = isWallRunning;
        //Debug.Log("JumpingEARLY: " + jumping);
    }

    public void LateUpdate()
    {  
        isWallRunning = false;

        //Debug.Log("Can Attach: " + CanAttach());
        //Debug.Log("Vertical Check: " + VerticalCheck());
        //Debug.Log("Can Wall Run: " + CanWallRun());
        //Debug.Log("JumpingLATE: " + jumping);
        
        
        //if(Input.GetKeyDown("space")) //if pressed jump button
        //{
        //    jumping = true;
        //}

        //if (CharacterControllerScript.characterController.isGrounded)
        //{
        //    if (!prevGrounded) //wasnt grounded previously
        //        jumping = true;
            //else
            //    jumping = false;
        //}

        if(CanAttach())
        {
            hits = new RaycastHit[directions.Length];

            for(int i=0; i<directions.Length; i++)
            {
                Vector3 dir = transform.TransformDirection(directions[i]);
                Physics.Raycast(transform.position, dir, out hits[i], wallMaxDistance);
                if(hits[i].collider != null)
                {
                    Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(transform.position, dir * wallMaxDistance, Color.red);
                }
            }

            if(CanWallRun())
            {   
                hits = hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
                if(hits.Length > 0)
                {
                    OnWall(hits[0]);
                    lastWallPosition = hits[0].point;
                    lastWallNormal = hits[0].normal;
                }
            }
        }

        if(isWallRunning)
        {
            elapsedTimeSinceWallDetatch = 0;
            if(elapsedTimeSinceWallAttach == 0 && wallRunVolume != null)
            {
                lastVolumeValue = wallRunVolume.weight;
            }
            elapsedTimeSinceWallAttach += Time.deltaTime;
            
            //CharacterControllerScript.characterController.Move(Vector3.down * wallGravityDownForce * 0.1f * Time.deltaTime); //accelerate downwards slowly - so you dont wall run infinetly OLDMOVE
        }
        else
        {   
            elapsedTimeSinceWallAttach = 0;
            if(elapsedTimeSinceWallDetatch == 0 && wallRunVolume != null)
            {
                lastVolumeValue = wallRunVolume.weight;
            }
            elapsedTimeSinceWallDetatch += Time.deltaTime;
        }

        if(wallRunVolume != null)
        {
            HandleVolume();
        }
        prevGrounded = CharacterControllerScript.characterController.isGrounded;
    }

    bool CanAttach()
    {
        if(jumping)
        {
            elapsedTimeSinceJump += Time.deltaTime;
            if(elapsedTimeSinceJump > jumpDuration)
            {
                elapsedTimeSinceJump = 0;
                jumping = false;
            }
            return false;
        }
        
        return true;
    }

    void OnWall(RaycastHit hit){
        float d = Vector3.Dot(hit.normal, Vector3.up);
        if(d >= -normalizedAngleThreshold && d <= normalizedAngleThreshold)
        {
            // Vector3 alongWall = Vector3.Cross(hit.normal, Vector3.up);
            //float vertical = Input.GetAxisRaw(GameConstants.k_AxisNameVertical);// ?????

            Vector3 alongWall = transform.TransformDirection(Vector3.forward);

            Debug.DrawRay(transform.position, alongWall.normalized * 10, Color.green);
            Debug.DrawRay(transform.position, lastWallNormal * 10, Color.magenta);

            //m_PlayerCharacterController.characterVelocity = alongWall * vertical * wallSpeedMultiplier; //calculate velocity
            //CharacterControllerScript.characterController.Move(alongWall * 0.1f * wallSpeedMultiplier); //calculate velocity //OLDMOVE
            isWallRunning = true;
        }
    }

    float CalculateSide()
    {
        if(isWallRunning)
        {
            Vector3 heading = lastWallPosition - transform.position;
            Vector3 perp = Vector3.Cross(transform.forward, heading);
            float dir = Vector3.Dot(perp, transform.up);
            return dir;
        }
        return 0;
    }

    public float GetCameraRoll()
    {
        float dir = CalculateSide();
        //float cameraAngle = m_PlayerCharacterController.playerCamera.transform.eulerAngles.z; //get camera z axis
        float cameraAngle = CharacterControllerScript.instance.pCam.transform.eulerAngles.z;
        float targetAngle = 0;
        if(dir != 0)
        {
            targetAngle = Mathf.Sign(dir) * maxAngleRoll;
        }
        return Mathf.LerpAngle(cameraAngle, targetAngle, Mathf.Max(elapsedTimeSinceWallAttach, elapsedTimeSinceWallDetatch) / cameraTransitionDuration);
    } 

    public Vector3 GetWallJumpDirection()
    {
        if(isWallRunning)
        {
            return lastWallNormal * wallBouncing + Vector3.up;
        }
        return Vector3.zero;
    } 

    void HandleVolume()
    {
        float w = 0;
        if(isWallRunning)
        {
            w = Mathf.Lerp(lastVolumeValue, 1, elapsedTimeSinceWallAttach / cameraTransitionDuration);
        }
        else
        {
            w = Mathf.Lerp(lastVolumeValue, 0, elapsedTimeSinceWallDetatch / cameraTransitionDuration);
        }

        SetVolumeWeight(w);
    }

    void SetVolumeWeight(float weight)
    {
        wallRunVolume.weight = weight;
    }
}
