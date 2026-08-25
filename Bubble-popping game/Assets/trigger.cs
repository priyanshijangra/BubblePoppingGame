using UnityEngine;

public class BubbleTrigger : MonoBehaviour
{
    [Tooltip("Drag the BubbleManager object here.")]
    public BubbleManager bubbleManager;

    [Header("Audio")]
    public AudioSource popSound;

    [Header("Visuals (optional)")]
    public Sprite normalBubble;
    public Sprite poppedBubble;

    private SpriteRenderer spriteRenderer;
    private bool isPopped = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Reset state every time this bubble is (re)activated.
        isPopped = false;
        if (spriteRenderer != null && normalBubble != null)
            spriteRenderer.sprite = normalBubble;

        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPopped) return;

        // Same detection pattern as your TargetTrigger script.
        if (other.GetComponentInParent<Leap.PhysicalHands.ContactBone>() != null)
        {
            Pop();
        }
    }

    void Pop()
{
    isPopped = true;

    if (bubbleManager != null && bubbleManager.popSound != null)
        bubbleManager.popSound.Play();

    if (spriteRenderer != null && poppedBubble != null) spriteRenderer.sprite = poppedBubble;
    if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

    if (bubbleManager != null)
        bubbleManager.OnBubblePopped(gameObject);
}
}