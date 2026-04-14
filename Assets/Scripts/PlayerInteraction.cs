using UnityEngine;

/// <summary>
/// PlayerInteractionConsole
/// - Deteksi NPC terdekat dalam radius
/// - Saat masuk radius => tampilkan prompt (ke Console) sekali saat entry
/// - Tekan F => panggil Interact dan tampilkan inventory result di Console
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("Radius interaksi (meter)")]
    public float interactRadius = 2f;

    [Tooltip("Layer untuk NPC interaktif (Muzaki/Mustahik)")]
    public LayerMask interactLayer;

    // state untuk mencegah spam log setiap frame
    private GameObject currentlyPromptedObject = null;
    private string lastPromptMessage = "";

    void Update()
    {
        DetectAndPrompt();

        if (Input.GetKeyDown(KeyCode.F) && currentlyPromptedObject != null)
        {
            // Ambil komponen IInteractable (implementer seperti Muzaki/Mustahik)
            var interactable = currentlyPromptedObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // capture inventory before
                int before = InventoryManager.Instance != null ? InventoryManager.Instance.CurrentZakat : -1;

                // call interact
                interactable.Interact(gameObject);

                // capture inventory after
                int after = InventoryManager.Instance != null ? InventoryManager.Instance.CurrentZakat : -1;

                // Determine what happened and print result
                if (before >= 0 && after >= 0)
                {
                    if (after > before)
                    {
                        Debug.Log($"[ZAKAT] Collected {after - before} zakat. Inventory now: {after}.");
                    }
                    else if (after < before)
                    {
                        Debug.Log($"[ZAKAT] Gave {before - after} zakat. Inventory now: {after}.");
                    }
                    else
                    {
                        // tidak berubah -> kemungkinan inventory penuh atau tidak cukup
                        // coba lebih spesifik jika target adalah Mustahik atau Muzaki
                        if (currentlyPromptedObject.GetComponent<Muzaki>() != null)
                        {
                            Debug.Log("[ZAKAT] Cannot collect: Inventory may be full.");
                        }
                        else if (currentlyPromptedObject.GetComponent<Mustahik>() != null)
                        {
                            // ambil required amount dari mustahik (jika tersedia)
                            var m = currentlyPromptedObject.GetComponent<Mustahik>();
                            if (m != null)
                                Debug.Log($"[ZAKAT] Cannot give: Not enough zakat to give {m.GetRequiredAmount()}.");
                            else
                                Debug.Log("[ZAKAT] Cannot give: Not enough zakat.");
                        }
                        else
                        {
                            Debug.Log("[ZAKAT] Interaction had no effect.");
                        }
                    }
                }
                else
                {
                    Debug.Log("[ZAKAT] InventoryManager not found or inventory unavailable.");
                }
            }
        }
    }

    void DetectAndPrompt()
    {
        Vector3 origin = transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, interactRadius, interactLayer, QueryTriggerInteraction.Collide);

        GameObject nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var c in hits)
        {
            // pastikan objek memiliki IInteractable
            var ia = c.GetComponent<IInteractable>();
            if (ia == null) continue;

            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < nearestDist)
            {
                nearest = c.gameObject;
                nearestDist = d;
            }
        }

        // jika berubah objek yang diprompt -> tampilkan prompt sekali
        if (nearest != currentlyPromptedObject)
        {
            currentlyPromptedObject = nearest;
            lastPromptMessage = "";

            if (currentlyPromptedObject != null)
            {
                // tentukan tipe prompt berdasarkan komponen Muzaki / Mustahik
                var muz = currentlyPromptedObject.GetComponent<Muzaki>();
                var mus = currentlyPromptedObject.GetComponent<Mustahik>();

                if (muz != null)
                {
                    int amt = muz.GetZakatAmount();
                    lastPromptMessage = $"[PROMPT] Press F to collect {amt} zakat from Muzaki (nearby).";
                    Debug.Log(lastPromptMessage);
                }
                else if (mus != null)
                {
                    int req = mus.GetRequiredAmount();
                    lastPromptMessage = $"[PROMPT] Press F to give {req} zakat to Mustahik (nearby).";
                    Debug.Log(lastPromptMessage);
                }
                else
                {
                    lastPromptMessage = $"[PROMPT] Press F to interact.";
                    Debug.Log(lastPromptMessage);
                }
            }
            else
            {
                // left all interactables
                Debug.Log("[PROMPT] No interactable nearby.");
            }
        }
        // else: tetap pada object yang sama -> tidak spam prompt
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
