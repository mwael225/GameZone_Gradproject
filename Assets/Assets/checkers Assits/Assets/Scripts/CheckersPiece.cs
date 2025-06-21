using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class CheckersPiece : NetworkBehaviour
{
    private int boardSize = 8;
    private int rowsPerPlayer = 3;
    public bool isSelected = false;
    public NetworkVariable<bool> isPlayerOne = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isKing = new NetworkVariable<bool>(false);
    public GameObject crownPrefab;
    private GameObject crownInstance;

    private Renderer rend;
    private Color originalColor;
    public Color selectedColor = Color.yellow;
    public Color playerOneColor = Color.white;
    public Color playerTwoColor = Color.black;

    private Vector3 regularScale = new Vector3(10f, 10f, 10f);
    
    // Animation parameters
    private bool isMoving = false;
    private float moveSpeed = 10f;
    private float jumpHeight = 2f;
    private Vector3 targetPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to network variable changes
        isPlayerOne.OnValueChanged += OnPlayerChanged;
        isKing.OnValueChanged += OnKingStateChanged;
        
        // Initialize the piece's state
        rend = GetComponent<Renderer>();
        OnPlayerChanged(false, isPlayerOne.Value); // Set initial color
        OnKingStateChanged(false, isKing.Value); // Check if it's already a king
        
        transform.localScale = regularScale;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        // Unsubscribe to prevent memory leaks
        isPlayerOne.OnValueChanged -= OnPlayerChanged;
        isKing.OnValueChanged -= OnKingStateChanged;
    }

    private void OnPlayerChanged(bool previousValue, bool newValue)
    {
        if (rend != null)
        {
            originalColor = newValue ? playerOneColor : playerTwoColor;
            rend.material.color = originalColor;
        }
    }

    private void OnKingStateChanged(bool previousValue, bool newValue)
    {
        if (newValue) // If isKing is true
        {
            SpawnCrown();
        }
        else // if isKing is false
        {
            if (crownInstance != null)
            {
                Destroy(crownInstance);
            }
        }
    }

    private void SpawnCrown()
    {
        if (crownPrefab != null && crownInstance == null)
        {
            crownInstance = Instantiate(crownPrefab, transform);
            crownInstance.transform.localPosition = new Vector3(0, 0.0095f, 0);
            crownInstance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            crownInstance.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

            StartCoroutine(AnimateCrownSpawn(crownInstance.transform));
        }
    }

    [ClientRpc]
    private void SpawnCrownClientRpc()
    {
        // This method is now obsolete because OnValueChanged handles it,
        // but we'll leave it for now to avoid breaking other parts. 
        // A better long-term solution would be to remove it.
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeKingServerRpc()
    {
        if (!isKing.Value)
        {
            isKing.Value = true;
        }
    }

    public void MakeKing()
    {
        if (IsServer)
        {
            isKing.Value = true;
        }
        else
        {
            MakeKingServerRpc();
        }
    }

    private IEnumerator AnimateCrownSpawn(Transform crown)
    {
        // Wait before starting the animation
        yield return new WaitForSeconds(0.5f);
        
        // Start from a smaller scale
        crown.localScale = Vector3.zero;
        
        // Animate to target scale
        Vector3 targetScale = new Vector3(0.02f, 0.02f, 0.02f);
        float duration = 1.5f; // Increased duration
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Add a more pronounced bounce effect
            float scale = Mathf.Sin(progress * Mathf.PI) * 1.3f;
            if (scale > 1f) scale = 2f - scale;
            
            // Slower initial rise, faster at the end
            float easedProgress = Mathf.SmoothStep(0, 1, scale);
            
            crown.localScale = targetScale * easedProgress;
            yield return null;
        }
        
        // Add a final small bounce
        float bounceTime = 0.3f;
        float bounceElapsed = 0f;
        
        while (bounceElapsed < bounceTime)
        {
            bounceElapsed += Time.deltaTime;
            float bounceProgress = bounceElapsed / bounceTime;
            float bounce = 1f + Mathf.Sin(bounceProgress * Mathf.PI * 2) * 0.1f;
            
            crown.localScale = targetScale * bounce;
            yield return null;
        }
        
        crown.localScale = targetScale;
    }

    public void MoveTo(Vector3 newPosition)
    {
        if (!isMoving)
        {
            targetPosition = newPosition;
            StartCoroutine(AnimateMove());
        }
    }

    private IEnumerator AnimateMove()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (transform.position != targetPosition)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            
            // Add a jumping arc to the movement
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            float jumpOffset = jumpHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);
            currentPosition.y += jumpOffset;
            
            transform.position = currentPosition;

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public void Select()
    {
        isSelected = true;
        if (rend != null)
            rend.material.color = selectedColor;
    }

    public void Deselect()
    {
        isSelected = false;
        if (rend != null)
            rend.material.color = originalColor;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}