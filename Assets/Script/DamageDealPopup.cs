using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageDealPopup : MonoBehaviour
{
    [SerializeField]
    private float disappearTimer = 4f;

    [SerializeField]
    private float disappearSpeed = 3f;

    [SerializeField]
    private Color textColor = Color.white;

    [SerializeField]
    private Color criticalHitColor = Color.red;

    [SerializeField]
    private int criticalHitFontSize = 14;

    [SerializeField]
    private float moveVectorSpeed = 30f;

    [SerializeField]
    private const float DISAPPEAR_TMER_MAX = 1f;
    [SerializeField]
    private const float INCREASE_SCALE_AMOUNT = 1f;
    [SerializeField]
    private const float DECREASE_SCALE_AMOUNT = 1f;

    private TextMeshPro textMesh;
    private Vector3 moveVector;
    private static int sortingOrder;

    public static DamageDealPopup Crate(Transform damageDealPopupTransform, Vector3 position, int damageAmount, bool isCriticalHit)
    {
        Transform damagePopupTransform = Instantiate(damageDealPopupTransform, position, Quaternion.identity);
        DamageDealPopup damageDealPopup = damagePopupTransform.GetComponent<DamageDealPopup>();
        damageDealPopup.Setup(damageAmount, isCriticalHit);

        return damageDealPopup;
    }

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>(); 
    }

    public void Setup(int damageAmount, bool isCriticalHit)
    {
        textMesh.SetText(damageAmount.ToString());

        if(isCriticalHit) {
            textMesh.fontSize = criticalHitFontSize;
            textMesh.color = criticalHitColor;
        }
        
        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TMER_MAX;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        moveVector = new Vector3(.7f, 1) * moveVectorSpeed;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if(disappearTimer > DISAPPEAR_TMER_MAX * .5f)
        {
            transform.localScale += Vector3.one * INCREASE_SCALE_AMOUNT * Time.deltaTime;
        }
        else
        {
            transform.localScale -= Vector3.one * DECREASE_SCALE_AMOUNT * Time.deltaTime;
        }


        disappearTimer -= Time.deltaTime;
        if(disappearTimer < 0)
        {
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if(textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
