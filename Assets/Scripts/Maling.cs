using UnityEngine;
using UnityEngine.AI; 
using System.Collections; 

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class Maling : MonoBehaviour, IInteractable
{
    [Header("Debug / Visual")]
    public GameObject fleeTargetMarkerPrefab;   
    private GameObject fleeTargetMarkerInstance;

    [Header("AI Ranges")]
    public float chaseRange = 5f;
    public float stealRange = 1f;
    public float fleeDistance = 8f;
    public float fleeTime = 3f;

    [Header("Chase Settings")]
    public float maxChaseDuration = 5f; 
    private float currentChaseTimer = 0f; 

    [Header("Speeds")]
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;

    [Header("Steal Settings")]
    public int maxStealPerHit = 3;

    [Header("Reclaim / Cooldown")]
    public float reclaimCooldown = 5f;
    public float restDuration = 5f; 

    [Header("Optional Visuals")]
    public GameObject stolenIndicator;

    [Header("SFX Maling")]
    public AudioClip RunStepAudio;
    public AudioClip sfxMencuriZakat;
    AudioSource PlayerAudio;

    private NavMeshAgent agent;  
    private Transform player;
    private float stolenAmount = 0f;
    private Animator animator;

    private enum State { Idle, Chasing, StealAndFlee, Fleeing, IdleAfterFlee, AttemptingSteal }
    private State state = State.Idle;

    private Vector3 fleeTarget;
    private float fleeTimer = 0f;
    private bool inCooldown = false;
    private float cooldownTimer = 0f;
    private bool hasFleeTarget = false;
    private int fleeRetryCount = 0;
    private const int maxFleeRetries = 10; 

    public bool IsInvincible { get; private set; } = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); 
        animator = GetComponent<Animator>(); 
        agent.speed = chaseSpeed; 
        agent.stoppingDistance = stealRange; 

        var pgo = GameObject.FindWithTag("Player");
        if (pgo == null) pgo = GameObject.Find("Player");
        if (pgo != null) player = pgo.transform;
    }

    private void Start()
    {
        UpdateStolenVisual();
        PlayerAudio = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (inCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                inCooldown = false;
                cooldownTimer = 0f;
                Debug.Log("[THIEF] Cooldown ended, thief can chase again.");
            }
        }

        if (player == null || InventoryManager.Instance == null)
            return;

        int playerZakat = InventoryManager.Instance.CurrentZakat;

        switch (state)
        {
            case State.Idle:
                if (!inCooldown && playerZakat >= 1 && Vector3.Distance(transform.position, player.position) <= chaseRange)
                {
                    state = State.Chasing;
                    currentChaseTimer = 0f; 
                }
                break;

            case State.Chasing:
                // --- PERBAIKAN: Pastikan Shoulder Rubbing Mati saat mengejar ---
                animator.SetBool("Shoulder Rubbing", false);
                // ---------------------------------------------------------------

                if (playerZakat < 1)
                {
                    state = State.Idle;
                    agent.isStopped = true; 
                    animator.SetBool("Running", false);
                    animator.SetBool("Shoulder Rubbing", true);
                    break;
                }

                currentChaseTimer += Time.deltaTime; 
                if (currentChaseTimer >= maxChaseDuration)
                {
                    Debug.Log("[THIEF] Capek mengejar (Waktu Habis). Istirahat dulu.");
                    EnterRestCooldown(); 
                    break; 
                }

                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= stealRange)
                {
                    if (!inCooldown)
                    {
                        TryInitiateStealQTE();
                    }
                    else
                    {
                        state = State.Idle;
                        agent.isStopped = true;
                        animator.SetBool("Running", false);
                        animator.SetBool("Shoulder Rubbing", true);
                    }
                }
                else
                {
                    agent.isStopped = false; 
                    agent.SetDestination(player.position); 
                    animator.SetBool("Running", true);
                    animator.SetBool("Shoulder Rubbing", false);
                }
                break;
            
            case State.AttemptingSteal:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetBool("Running", false);
                // Pastikan tidak rubbing saat QTE
                animator.SetBool("Shoulder Rubbing", false); 
                break;

            case State.Fleeing:
                // --- PERBAIKAN: Force Animation Lari ---
                animator.SetBool("Running", true);
                animator.SetBool("Shoulder Rubbing", false);
                // ---------------------------------------

                if (fleeTarget == Vector3.zero) 
                {
                    StartFlee(); 
                }

                if (agent.hasPath) 
                {
                     agent.isStopped = false;
                }

                // --- PERBAIKAN LOGIKA STOP ---
                // Hanya cek jarak jika kita SUDAH punya target yang valid (hasFleeTarget)
                // Ini mencegah maling langsung berhenti di frame pertama karena path belum dihitung
                if (hasFleeTarget) 
                {
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                    {
                        EnterRestCooldown(); 
                    }
                }

                fleeTimer += Time.deltaTime;
                if (fleeTimer >= fleeTime) 
                {
                    EnterRestCooldown();
                }
                break;

            case State.IdleAfterFlee:
                agent.isStopped = true;
                animator.SetBool("Running", false);
                animator.SetBool("Shoulder Rubbing", true);

                if (!inCooldown)
                {
                    if (stolenAmount > 0)
                    {
                        fleeTarget = Vector3.zero;
                        hasFleeTarget = false;
                        state = State.Fleeing; 
                    }
                    else if (playerZakat >= 1 && Vector3.Distance(transform.position, player.position) <= chaseRange)
                    {
                        state = State.Chasing;
                        currentChaseTimer = 0f; 
                        animator.SetBool("Running", true);
                        animator.SetBool("Shoulder Rubbing", false);
                    }
                }
                break;
        }
    }

    private void TryInitiateStealQTE()
    {
        state = State.AttemptingSteal;
        // Matikan animasi idle jika ada
        animator.SetBool("Shoulder Rubbing", false);

        PlayerLogic logic = player.GetComponent<PlayerLogic>();
        if (logic != null)
        {
            Debug.Log("[THIEF] Mencoba mencuri... Memulai QTE.");
            logic.StartStruggleQTE(2f, 10, OnQTESuccess, OnQTEFail);
        }
        else
        {
            ActualSteal();
        }
    }

    private void OnQTESuccess()
    {
        Debug.Log("[THIEF] Gagal mencuri karena dihalau player!");
        animator.SetTrigger("Shoved"); 
        EnterRestCooldown(); 
    }

    private void OnQTEFail()
    {
        Debug.Log("[THIEF] Berhasil mencuri karena player lambat!");
        ActualSteal();
        IsInvincible = true;
    }

    private void ActualSteal()
    {
        animator.SetTrigger("Pick Up Item");
        
        // --- PERBAIKAN: Matikan Shoulder Rubbing secara eksplisit ---
        animator.SetBool("Shoulder Rubbing", false);
        // ------------------------------------------------------------

        int current = InventoryManager.Instance.CurrentZakat;
        if (current <= 0) 
        {
            state = State.Idle;
            return;
        }

        int amountToSteal = Random.Range(1, Mathf.Min(current, maxStealPerHit) + 1);
        bool ok = InventoryManager.Instance.RemoveZakat(amountToSteal);
        if (ok)
        {
            stolenAmount = amountToSteal;
            
            if (sfxMencuriZakat != null && PlayerAudio != null)
            {
                PlayerAudio.PlayOneShot(sfxMencuriZakat);
            } 
            
            UpdateStolenVisual();

            fleeTarget = Vector3.zero;
            hasFleeTarget = false;

            state = State.Fleeing;
        }
    }

    private void StartFlee()
    {
        animator.SetBool("Running", true);
        animator.SetBool("Shoulder Rubbing", false);
        if (player == null || agent == null)
            return;

        fleeTimer = 0f;
        hasFleeTarget = false;

        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < maxFleeRetries; i++)
        {
            Vector3 dirFromPlayer = transform.position - player.position;
            dirFromPlayer.y = 0f;

            if (dirFromPlayer.sqrMagnitude < 0.001f)
            {
                dirFromPlayer = Random.insideUnitSphere;
                dirFromPlayer.y = 0f;
            }

            dirFromPlayer.Normalize();

            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f), 
                0f,
                Random.Range(-1f, 1f)
            );

            Vector3 finalDir = (dirFromPlayer + randomOffset).normalized;
            Vector3 desiredPos = transform.position + finalDir * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredPos, out hit, fleeDistance, NavMesh.AllAreas))
            {
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    fleeTarget = hit.position;
                    hasFleeTarget = true;
                    break; 
                }
            }
        }

        if (!hasFleeTarget)
        {
            if (fleeRetryCount < maxFleeRetries)
            {
                fleeRetryCount++;
                StartFlee();  
            }
            else
            {
                fleeRetryCount = 0; 
                state = State.Idle; 
            }
            return;
        }

        agent.speed = fleeSpeed;
        agent.isStopped = false;
        agent.SetDestination(fleeTarget);
        UpdateFleeMarker();
    }

    private void UpdateStolenVisual()
    {
        if (stolenIndicator != null)
            stolenIndicator.SetActive(stolenAmount > 0);
    }

    public void Interact(GameObject interactor)
    {
        if (IsInvincible)
        {
            Debug.Log("[THIEF] Maling sedang kebal (Invincible)!");
            return; 
        }

        if (stolenAmount <= 0)
        {
            Debug.Log("[THIEF] Tidak ada zakat untuk direbut.");
            return;
        }

        int returned = InventoryManager.Instance.AddZakat((int)stolenAmount, false);
        
        animator.SetTrigger("Shoved");
        StartCoroutine(WaitForShovedAnimation());
        if (returned > 0)
        {
            Debug.Log($"[THIEF] Player berhasil merebut {returned} zakat dari maling!");
            stolenAmount -= returned;
            if (stolenAmount <= 0)
            {
                stolenAmount = 0;
                UpdateStolenVisual();
            }
            EnterCooldown();
        }
        else
        {
            Debug.Log("[THIEF] Inventory player penuh, tidak bisa merebut zakat.");
        }
    }

    private IEnumerator WaitForShovedAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length); 

        animator.SetBool("Running", false); 
        animator.SetBool("Shoulder Rubbing", true); 
    }

    private void EnterRestCooldown()
    {
        if (state == State.IdleAfterFlee && inCooldown) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero; 
        agent.ResetPath(); 
        
        animator.SetBool("Running", false);
        animator.SetBool("Shoulder Rubbing", true);
        
        state = State.IdleAfterFlee;
        ClearFleeMarker();

        inCooldown = true;
        cooldownTimer = restDuration;
        
        if (IsInvincible)
        {
            IsInvincible = false;
        }

        Debug.Log($"[THIEF] Masuk fase istirahat selama {restDuration} detik.");
    }

    public void EnterCooldown()
    {
        inCooldown = true;
        cooldownTimer = reclaimCooldown;
        state = State.IdleAfterFlee;
        ClearFleeMarker();
        
        IsInvincible = false; 
    }

    public int GetStolenAmount()
    {
        return Mathf.RoundToInt(stolenAmount);
    }

    private void UpdateFleeMarker()
    {
        if (fleeTargetMarkerPrefab == null) return;
        if (fleeTargetMarkerInstance == null)
        {
            fleeTargetMarkerInstance = Instantiate(fleeTargetMarkerPrefab);
        }
        fleeTargetMarkerInstance.transform.position = fleeTarget + Vector3.up * 0.1f;
        fleeTargetMarkerInstance.SetActive(true);
    }

    private void ClearFleeMarker()
    {
        if (fleeTargetMarkerInstance != null)
        {
            Destroy(fleeTargetMarkerInstance);
            fleeTargetMarkerInstance = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stealRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }

    public void runstep()
    {
        PlayerAudio.clip = RunStepAudio;
        PlayerAudio.Play();
    }
}