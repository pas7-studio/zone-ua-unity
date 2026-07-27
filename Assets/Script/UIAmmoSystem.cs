using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIAmmoSystem : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject ammoEnabledPrefab;
    [SerializeField] private GameObject ammoDisabledPrefab;

    [Header("Parents")]
    [SerializeField] private Transform ammoEnabledParent;
    [SerializeField] private Transform ammoDisabledParent;
    [SerializeField] private Transform ammoCounter;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI ammoCounterTextPro;

    private readonly List<Image> enabledAmmoImages = new List<Image>();
    private readonly List<GameObject> disabledAmmoImages = new List<GameObject>();
    private int visibleMaximum;

    private void Awake()
    {
        if (ammoCounterTextPro == null && ammoCounter != null)
        {
            ammoCounterTextPro = ammoCounter.GetComponent<TextMeshProUGUI>();
        }

        RegisterExistingChildren();
    }

    private void RegisterExistingChildren()
    {
        enabledAmmoImages.Clear();
        disabledAmmoImages.Clear();

        if (ammoEnabledParent != null)
        {
            for (int i = 0; i < ammoEnabledParent.childCount; i++)
            {
                Image image = ammoEnabledParent.GetChild(i).GetComponent<Image>();
                if (image != null)
                {
                    enabledAmmoImages.Add(image);
                }
            }
        }

        if (ammoDisabledParent != null)
        {
            for (int i = 0; i < ammoDisabledParent.childCount; i++)
            {
                disabledAmmoImages.Add(ammoDisabledParent.GetChild(i).gameObject);
            }
        }
    }

    public void SetMaximumAmmo(int maximumAmmo)
    {
        maximumAmmo = Mathf.Max(0, maximumAmmo);
        EnsureCapacity(maximumAmmo);
        visibleMaximum = maximumAmmo;

        for (int i = 0; i < enabledAmmoImages.Count; i++)
        {
            bool visible = i < visibleMaximum;
            enabledAmmoImages[i].gameObject.SetActive(visible);

            if (i < disabledAmmoImages.Count)
            {
                disabledAmmoImages[i].SetActive(visible);
            }
        }

        SetCounter(0, maximumAmmo);
    }

    public void SetAmmo(int currentAmmo, int maximumAmmo)
    {
        maximumAmmo = Mathf.Max(0, maximumAmmo);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maximumAmmo);

        if (visibleMaximum != maximumAmmo)
        {
            SetMaximumAmmo(maximumAmmo);
        }

        int spentAmmo = maximumAmmo - currentAmmo;
        for (int i = 0; i < maximumAmmo && i < enabledAmmoImages.Count; i++)
        {
            enabledAmmoImages[i].enabled = i >= spentAmmo;
        }

        SetCounter(currentAmmo, maximumAmmo);
    }

    public void ReloadAmmo(int maximumAmmo)
    {
        SetAmmo(maximumAmmo, maximumAmmo);
    }

    public void PopAmmo(int currentAmmo, int maximumAmmo)
    {
        SetAmmo(currentAmmo, maximumAmmo);
    }

    public void ShowHideUI(bool state)
    {
        if (ammoEnabledParent != null)
        {
            ammoEnabledParent.gameObject.SetActive(state);
        }

        if (ammoDisabledParent != null)
        {
            ammoDisabledParent.gameObject.SetActive(state);
        }

        if (ammoCounterTextPro != null)
        {
            ammoCounterTextPro.enabled = state;
        }
    }

    private void EnsureCapacity(int required)
    {
        while (enabledAmmoImages.Count < required)
        {
            if (ammoEnabledPrefab == null || ammoEnabledParent == null)
            {
                break;
            }

            GameObject instance = Instantiate(ammoEnabledPrefab, ammoEnabledParent);
            Image image = instance.GetComponent<Image>();
            if (image == null)
            {
                Destroy(instance);
                break;
            }

            enabledAmmoImages.Add(image);
        }

        while (disabledAmmoImages.Count < required)
        {
            if (ammoDisabledPrefab == null || ammoDisabledParent == null)
            {
                break;
            }

            GameObject instance = Instantiate(ammoDisabledPrefab, ammoDisabledParent);
            disabledAmmoImages.Add(instance);
        }
    }

    private void SetCounter(int currentAmmo, int maximumAmmo)
    {
        if (ammoCounterTextPro != null)
        {
            ammoCounterTextPro.SetText("{0}/{1}", currentAmmo, maximumAmmo);
        }
    }
}
