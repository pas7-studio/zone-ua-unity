using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public sealed class DamageDealPopup : MonoBehaviour
{
    private const float DisappearTimerMax = 1f;
    private const float IncreaseScaleAmount = 1f;
    private const float DecreaseScaleAmount = 1f;

    [SerializeField, Min(0f)] private float disappearTimer = 4f;
    [SerializeField, Min(0f)] private float disappearSpeed = 3f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color criticalHitColor = Color.red;
    [SerializeField, Min(1)] private int criticalHitFontSize = 14;
    [SerializeField, Min(0f)] private float moveVectorSpeed = 30f;

    private static int sortingOrder;

    private TextMeshPro textMesh;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public static DamageDealPopup Create(
        Transform popupPrefab,
        Vector3 position,
        int damageAmount,
        bool isCriticalHit)
    {
        if (popupPrefab == null)
        {
            return null;
        }

        Transform popupTransform = Instantiate(popupPrefab, position, Quaternion.identity);
        DamageDealPopup popup = popupTransform.GetComponent<DamageDealPopup>();
        popup?.Setup(damageAmount, isCriticalHit);
        return popup;
    }

    // Backwards-compatible typo retained for existing callers.
    public static DamageDealPopup Crate(
        Transform popupPrefab,
        Vector3 position,
        int damageAmount,
        bool isCriticalHit)
    {
        return Create(popupPrefab, position, damageAmount, isCriticalHit);
    }

    public void Setup(int damageAmount, bool isCriticalHit)
    {
        textMesh.SetText("{0}", damageAmount);

        if (isCriticalHit)
        {
            textMesh.fontSize = criticalHitFontSize;
            textMesh.color = criticalHitColor;
        }

        textColor = textMesh.color;
        disappearTimer = DisappearTimerMax;
        textMesh.sortingOrder = ++sortingOrder;
        moveVector = new Vector3(0.7f, 1f) * moveVectorSpeed;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector = Vector3.Lerp(moveVector, Vector3.zero, 8f * Time.deltaTime);

        float scaleDirection = disappearTimer > DisappearTimerMax * 0.5f
            ? IncreaseScaleAmount
            : -DecreaseScaleAmount;

        transform.localScale += Vector3.one * scaleDirection * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer >= 0f)
        {
            return;
        }

        textColor.a -= disappearSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
