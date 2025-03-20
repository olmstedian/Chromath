using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimationManager : MonoBehaviour
{
    // Enhance animations to be more visible
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.4f; // Increased from 0.2f for better visibility
    [SerializeField] private float mergeDuration = 0.2f; // Increased from 0.1f
    [SerializeField] private float scaleDuration = 0.4f; // Increased from 0.3f
    [SerializeField] private float specialEffectDuration = 0.3f; // Increased from 0.15f
    
    [Header("Animation Enhancements")]
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve bounceCurve;
    [SerializeField] private float bounceAmount = 0.3f; // Increased from 0.2f
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject matchParticlePrefab;
    [SerializeField] private GameObject mergeParticlePrefab;
    [SerializeField] private GameObject specialTileParticlePrefab;
    
    [Header("Animation Visibility")]
    [SerializeField] private bool useRandomRotation = true;
    [SerializeField] private float maxRandomRotation = 25f; // Increased from 15f
    [SerializeField] private bool useScaleBounce = true;
    [SerializeField] private float arcMultiplier = 0.2f; // New setting to control arc height
    [SerializeField] private bool debugMode = true; // New debug flag to log animations

    // Singleton instance
    public static TileAnimationManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup default curves if not set in inspector
        if (bounceCurve.keys.Length == 0)
        {
            // Create a bounce curve
            Keyframe[] keys = new Keyframe[4];
            keys[0] = new Keyframe(0f, 0f, 0f, 0f);
            keys[1] = new Keyframe(0.5f, 1.1f, 0f, 0f);
            keys[2] = new Keyframe(0.75f, 0.95f, 0f, 0f);
            keys[3] = new Keyframe(1f, 1f, 0f, 0f);
            bounceCurve = new AnimationCurve(keys);
        }
    }
    
    #region Movement Animations
    
    // Enhanced tile movement animation with more pronounced arcs
    public IEnumerator MoveTileAnimation(GameObject tile, Vector3 targetPosition)
    {
        if (tile == null) yield break;
        
        if (debugMode) Debug.Log($"Starting tile movement animation for {tile.name} to {targetPosition}");
        
        Vector3 startPos = tile.transform.position;
        float elapsedTime = 0;
        
        // Optional: Add slight rotation during movement for visual variety
        Quaternion startRotation = tile.transform.rotation;
        Quaternion randomRotation = startRotation;
        
        if (useRandomRotation)
        {
            float randomAngle = Random.Range(-maxRandomRotation, maxRandomRotation);
            randomRotation = Quaternion.Euler(0, 0, randomAngle) * startRotation;
        }
        
        // Calculate a more pronounced arc for the movement path
        Vector3 midPoint = Vector3.Lerp(startPos, targetPosition, 0.5f);
        float arcHeight = Vector3.Distance(startPos, targetPosition) * arcMultiplier; // More pronounced arc
        midPoint += new Vector3(0, arcHeight, 0);
        
        // Add visual marker for movement start if in debug mode
        GameObject debugMarker = null;
        if (debugMode)
        {
            debugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            debugMarker.transform.position = startPos;
            debugMarker.GetComponent<Renderer>().material.color = Color.red;
            GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            endMarker.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            endMarker.transform.position = targetPosition;
            endMarker.GetComponent<Renderer>().material.color = Color.green;
            Destroy(endMarker, moveDuration + 0.1f);
        }
        
        while (elapsedTime < moveDuration)
        {
            if (tile == null) 
            {
                if (debugMarker != null) Destroy(debugMarker);
                yield break;
            }
            
            float t = elapsedTime / moveDuration;
            
            // Use the animation curve for smoother movement
            float curveT = movementCurve.Evaluate(t);
            
            // Path calculation (basic Bezier for arc movement)
            Vector3 m1 = Vector3.Lerp(startPos, midPoint, curveT);
            Vector3 m2 = Vector3.Lerp(midPoint, targetPosition, curveT);
            Vector3 position = Vector3.Lerp(m1, m2, curveT);
            
            // Apply position
            tile.transform.position = position;
            
            // Apply rotation if enabled - more pronounced
            if (useRandomRotation)
            {
                // Rotate during movement, then straighten at the end
                float rotationT = t < 0.8f ? Mathf.Min(t / 0.5f, 1f) : (1f - (t - 0.8f) / 0.2f);
                tile.transform.rotation = Quaternion.Slerp(startRotation, randomRotation, rotationT);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Clean up debug marker
        if (debugMarker != null) Destroy(debugMarker);
        
        // Ensure tile ends at exact position and rotation
        if (tile != null)
        {
            tile.transform.position = new Vector3(targetPosition.x, targetPosition.y, tile.transform.position.z);
            tile.transform.rotation = startRotation; // Reset to original rotation
            
            if (debugMode) Debug.Log($"Completed movement animation for {tile.name}");
        }
        
        // After completing movement, notify GameManager to spawn a new tile
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTileMovementComplete();
        }
    }
    
    // Enhanced animation for tile merging with particles
    public IEnumerator AnimateTileToMerge(GameObject tile, Vector3 targetPosition)
    {
        if (tile == null) yield break;
        
        Vector3 startPosition = tile.transform.position;
        float elapsedTime = 0f;
        
        // Store original scale for scaling during movement
        Vector3 originalScale = tile.transform.localScale;
        
        while (elapsedTime < mergeDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsedTime / mergeDuration;
            float curveT = movementCurve.Evaluate(t);
            
            // Move the tile
            tile.transform.position = Vector3.Lerp(startPosition, targetPosition, curveT);
            
            // Gradually scale down as it approaches the target
            tile.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.7f, curveT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Make sure the tile ends at the exact target position
        if (tile != null)
        {
            tile.transform.position = targetPosition;
            tile.transform.localScale = originalScale * 0.7f;
            
            // Spawn merge particle effect
            SpawnParticleEffect(mergeParticlePrefab, targetPosition);
        }
    }
    
    #endregion
    
    #region Scale Animations
    
    // More pronounced scale animation for merging
    public IEnumerator MergeTileAnimation(GameObject tile)
    {
        if (tile == null) yield break;
        
        if (debugMode) Debug.Log($"Starting merge animation for {tile.name}");
        
        // Scale up and down animation with enhanced bounce effect
        Vector3 originalScale = tile.transform.localScale;
        Vector3 expandedScale = originalScale * 1.5f; // Increased from 1.2f for better visibility
        
        // First phase - scale up with bounce effect
        float elapsed = 0;
        while (elapsed < mergeDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / mergeDuration;
            
            // Use bounce curve for scale with more exaggeration
            float bounceScale = useScaleBounce ? 
                bounceCurve.Evaluate(t) : 
                scaleCurve.Evaluate(t);
            
            // Apply scale with more visible change
            tile.transform.localScale = Vector3.Lerp(originalScale, expandedScale, bounceScale);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Flash effect is more pronounced
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Bright flash with longer duration
            StartCoroutine(FlashTileColor(renderer, Color.white, 0.15f));
        }
        
        // Spawn particle effect at merge location
        SpawnParticleEffect(mergeParticlePrefab, tile.transform.position, 1.2f); // Larger effect
        
        // Second phase - scale back with bounce
        elapsed = 0;
        while (elapsed < mergeDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / mergeDuration;
            float easeT = scaleCurve.Evaluate(t);
            
            tile.transform.localScale = Vector3.Lerp(expandedScale, originalScale, easeT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale is correct
        if (tile != null)
        {
            tile.transform.localScale = originalScale;
            if (debugMode) Debug.Log($"Completed merge animation for {tile.name}");
        }
    }
    
    // Enhanced scale-in animation with rotation and bounce
    public IEnumerator ScaleTileIn(GameObject tile)
    {
        if (tile == null) yield break;
        
        // Start with a tiny scale
        Vector3 originalScale = tile.transform.localScale;
        tile.transform.localScale = originalScale * 0.1f;
        
        // Store original rotation
        Quaternion originalRotation = tile.transform.rotation;
        
        // Random starting rotation (optional)
        if (useRandomRotation)
        {
            float randomAngle = Random.Range(-30f, 30f);
            tile.transform.rotation = Quaternion.Euler(0, 0, randomAngle);
        }
        
        float elapsed = 0;
        
        // Scale up to normal size with bounce effect
        while (elapsed < scaleDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / scaleDuration;
            
            // Apply bounce curve to the scale
            float bounceValue = useScaleBounce ? 
                bounceCurve.Evaluate(t) : 
                scaleCurve.Evaluate(t);
            
            // Scale with bounce effect
            float scaleMultiplier = Mathf.Lerp(0.1f, 1f, bounceValue);
            tile.transform.localScale = originalScale * scaleMultiplier;
            
            // Interpolate back to original rotation if using random rotation
            if (useRandomRotation)
            {
                tile.transform.rotation = Quaternion.Slerp(tile.transform.rotation, originalRotation, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale and rotation are correct
        if (tile != null)
        {
            tile.transform.localScale = originalScale;
            tile.transform.rotation = originalRotation;
        }
        
        // Add a subtle particle effect for appearing
        SpawnParticleEffect(matchParticlePrefab, tile.transform.position, 0.5f);
    }
    
    // Enhanced scale-out animation with rotation and particles
    public IEnumerator ScaleTileOut(GameObject tile)
    {
        if (tile == null) yield break;
        
        Vector3 originalScale = tile.transform.localScale;
        Quaternion originalRotation = tile.transform.rotation;
        float elapsed = 0;
        
        // Optional: Add a spin effect when disappearing
        float randomSpin = Random.Range(-180f, 180f);
        Quaternion targetRotation = Quaternion.Euler(0, 0, randomSpin) * originalRotation;
        
        // Scale down to nothing with optional spin
        while (elapsed < scaleDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / scaleDuration;
            
            // Use easing curve
            float curveT = scaleCurve.Evaluate(t);
            
            // Apply scale
            tile.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, curveT);
            
            // Apply rotation if enabled
            if (useRandomRotation)
            {
                tile.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, curveT);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Spawn a particle effect as the tile disappears
        SpawnParticleEffect(matchParticlePrefab, tile.transform.position);
    }
    
    #endregion
    
    #region Special Effects
    
    // Enhanced special tile creation effect with particles and glow
    public IEnumerator SpecialTileCreationEffect(GameObject tile)
    {
        if (tile == null) yield break;
        
        // Save original scale
        Vector3 originalScale = tile.transform.localScale;
        
        // Add a subtle rotation
        Quaternion originalRotation = tile.transform.rotation;
        Quaternion targetRotation1 = Quaternion.Euler(0, 0, 15f) * originalRotation;
        Quaternion targetRotation2 = Quaternion.Euler(0, 0, -15f) * originalRotation;
        
        // First pulse cycle
        float elapsed = 0f;
        while (elapsed < specialEffectDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / specialEffectDuration;
            float sin = Mathf.Sin(t * Mathf.PI);
            
            // Scale with bounce
            float scale = 1.0f + 0.3f * sin;
            tile.transform.localScale = originalScale * scale;
            
            // Add subtle rotation
            tile.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation1, sin);
            
            // Flash the color
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color baseColor = renderer.color;
                float brightness = 1f + 0.5f * sin;
                renderer.color = new Color(
                    baseColor.r * brightness,
                    baseColor.g * brightness,
                    baseColor.b * brightness,
                    baseColor.a
                );
            }
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Spawn special particle effect
        SpawnParticleEffect(specialTileParticlePrefab, tile.transform.position, 1f);
        
        // Second pulse cycle
        elapsed = 0f;
        while (elapsed < specialEffectDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsed / specialEffectDuration;
            float sin = Mathf.Sin(t * Mathf.PI);
            
            // Scale with bounce in the opposite direction
            float scale = 1.0f + 0.2f * sin;
            tile.transform.localScale = originalScale * scale;
            
            // Add subtle rotation in opposite direction
            tile.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation2, sin);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Ensure final scale and rotation are correct
        if (tile != null)
        {
            tile.transform.localScale = originalScale;
            tile.transform.rotation = originalRotation;
            
            // Reset color
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                // Ensure color is reset but maintain original alpha
                Color baseColor = renderer.color;
                renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a);
            }
        }
    }
    
    // Enhanced color flash animation with smoother transition
    public IEnumerator ColorFlashAnimation(GameObject tile, Color flashColor)
    {
        if (tile == null) yield break;
        
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;
        
        // Store original color
        Color originalColor = renderer.color;
        
        // Flash color with smooth transition
        float flashDuration = 0.2f;
        float elapsed = 0f;
        
        // Fade to flash color
        while (elapsed < flashDuration / 2)
        {
            if (tile == null || renderer == null) yield break;
            
            float t = elapsed / (flashDuration / 2);
            renderer.color = Color.Lerp(originalColor, flashColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Fade back to original
        elapsed = 0f;
        while (elapsed < flashDuration / 2)
        {
            if (tile == null || renderer == null) yield break;
            
            float t = elapsed / (flashDuration / 2);
            renderer.color = Color.Lerp(flashColor, originalColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure color is reset correctly
        if (tile != null && renderer != null)
        {
            renderer.color = originalColor;
        }
    }
    
    // Simplified flash without requiring a full coroutine
    public IEnumerator FlashTileColor(SpriteRenderer renderer, Color flashColor, float duration)
    {
        if (renderer == null) yield break;
        
        Color originalColor = renderer.color;
        
        // Quickly flash to the target color
        renderer.color = flashColor;
        
        // Wait for the flash duration
        yield return new WaitForSeconds(duration);
        
        // Return to the original color
        if (renderer != null)
        {
            renderer.color = originalColor;
        }
    }
    
    // Create a sparkle/particle effect at a position
    public void CreateMatchEffect(Vector3 position)
    {
        SpawnParticleEffect(matchParticlePrefab, position);
    }
    
    // Improved particle spawning with fallback visuals
    private void SpawnParticleEffect(GameObject particlePrefab, Vector3 position, float scale = 1f)
    {
        if (particlePrefab != null)
        {
            // Instantiate the particle effect
            GameObject effect = Instantiate(particlePrefab, position, Quaternion.identity);
            effect.transform.localScale *= scale;
            
            // Auto-destroy after the particle system completes
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                float duration = particles.main.duration + particles.main.startLifetimeMultiplier;
                Destroy(effect, duration);
                
                if (debugMode) Debug.Log($"Spawned particle effect at {position}, will destroy in {duration} seconds");
            }
            else
            {
                Destroy(effect, 2f);
            }
        }
        else if (debugMode)
        {
            // Fallback visual for debugging when prefabs aren't assigned
            GameObject fallbackEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fallbackEffect.transform.position = position;
            fallbackEffect.transform.localScale = Vector3.one * 0.2f * scale;
            
            // Add a simple animation
            StartCoroutine(AnimateFallbackEffect(fallbackEffect));
            
            Debug.LogWarning($"Using fallback effect at {position} - particle prefab not assigned");
        }
    }
    
    // Fallback animation for when particle prefabs aren't available
    private IEnumerator AnimateFallbackEffect(GameObject effect)
    {
        if (effect == null) yield break;
        
        // Random color
        Renderer renderer = effect.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f)
            );
        }
        
        // Scale animation
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (effect == null) yield break;
            
            float t = elapsed / duration;
            effect.transform.localScale = Vector3.Lerp(
                Vector3.one * 0.2f,
                Vector3.zero,
                t
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    #endregion
    
    #region Utility Methods
    
    // Add variation to a movement animation based on tile type/value
    public IEnumerator CustomizeTileMovement(GameObject tile, Vector3 targetPosition, float speed = 1.0f, bool useArc = true)
    {
        if (tile == null) yield break;
        
        // Check if it's a high-value or special tile
        GameTile gameTile = tile.GetComponent<GameTile>();
        bool isSpecial = gameTile != null && gameTile.IsSpecial();
        bool isHighValue = gameTile != null && gameTile.TileValue >= 8;
        
        // Adjust movement parameters based on tile properties
        float tileDuration = moveDuration / speed;
        float arcHeight = 0f;
        bool useGlow = isSpecial || isHighValue;
        
        if (useArc)
        {
            arcHeight = Vector3.Distance(tile.transform.position, targetPosition) * 0.15f;
            if (isHighValue) arcHeight *= 1.5f;
            if (isSpecial) arcHeight *= 2f;
        }
        
        // Add glow effect for special or high-value tiles
        if (useGlow)
        {
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                StartCoroutine(GlowDuringMovement(renderer, tileDuration));
            }
        }
        
        // Movement logic similar to MoveTileAnimation but with customizations
        Vector3 startPos = tile.transform.position;
        float elapsedTime = 0;
        
        // Mid-point for arc
        Vector3 midPoint = Vector3.Lerp(startPos, targetPosition, 0.5f);
        midPoint += new Vector3(0, arcHeight, 0); // Add height for arc
        
        while (elapsedTime < tileDuration)
        {
            if (tile == null) yield break;
            
            float t = elapsedTime / tileDuration;
            float curveT = movementCurve.Evaluate(t);
            
            Vector3 position;
            if (useArc && arcHeight > 0)
            {
                // Path calculation for arc movement
                Vector3 m1 = Vector3.Lerp(startPos, midPoint, curveT);
                Vector3 m2 = Vector3.Lerp(midPoint, targetPosition, curveT);
                position = Vector3.Lerp(m1, m2, curveT);
            }
            else
            {
                // Direct movement
                position = Vector3.Lerp(startPos, targetPosition, curveT);
            }
            
            // Apply position
            tile.transform.position = position;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure tile ends at exact position
        if (tile != null)
        {
            tile.transform.position = new Vector3(targetPosition.x, targetPosition.y, tile.transform.position.z);
        }
        
        // After completing movement, notify GameManager to spawn a new tile
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTileMovementComplete();
        }
    }
    
    // Add a glow effect during movement
    private IEnumerator GlowDuringMovement(SpriteRenderer renderer, float duration)
    {
        if (renderer == null) yield break;
        
        // Store original material
        Material originalMaterial = renderer.material;
        
        // Clone material to avoid affecting other objects
        Material glowMaterial = new Material(originalMaterial);
        renderer.material = glowMaterial;
        
        // Set glow parameters (assuming shader has _Glow property)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (renderer == null) yield break;
            
            float t = elapsed / duration;
            float glowIntensity = 0.5f * Mathf.Sin(t * Mathf.PI * 2f) + 0.5f;
            
            // Using standard shader, we can modify the color brightness
            Color baseColor = renderer.color;
            float brightness = 1f + 0.3f * glowIntensity;
            renderer.color = new Color(
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset material and color
        if (renderer != null)
        {
            renderer.material = originalMaterial;
        }
    }

    // You might want to add a method to set these references at runtime
    public void SetParticlePrefabs(GameObject matchPrefab, GameObject mergePrefab, GameObject specialPrefab)
    {
        matchParticlePrefab = matchPrefab;
        mergeParticlePrefab = mergePrefab;
        specialTileParticlePrefab = specialPrefab;
        
        Debug.Log("Particle prefabs assigned to TileAnimationManager");
    }
    
    #endregion

    // Remove the ParticleEffectGenerator reflection assignment method
    // Instead, add this method for direct prefab assignment via inspector
    private void OnValidate()
    {
        // Ensure we have valid animation curves
        if (movementCurve == null || movementCurve.keys.Length == 0)
        {
            movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        if (scaleCurve == null || scaleCurve.keys.Length == 0)
        {
            scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        // Make sure bounce curve is created
        if (bounceCurve == null || bounceCurve.keys.Length == 0)
        {
            SetupBounceCurve();
        }
    }
    
    private void SetupBounceCurve()
    {
        Keyframe[] keys = new Keyframe[4];
        keys[0] = new Keyframe(0f, 0f, 0f, 0f);
        keys[1] = new Keyframe(0.5f, 1.2f, 0f, 0f); // Higher bounce peak
        keys[2] = new Keyframe(0.75f, 0.9f, 0f, 0f);
        keys[3] = new Keyframe(1f, 1f, 0f, 0f);
        bounceCurve = new AnimationCurve(keys);
    }
}
