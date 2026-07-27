using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAmmoSystem : MonoBehaviour
{
    public GameObject ammoEnabledPrefab;
    public GameObject ammoDisabledPrefab;
    public Transform ammoEnabledParent;
    public Transform ammoDisabledParent;
    public Transform ammoCounter;

    [SerializeField]
    private TextMeshProUGUI ammoCounterTextPro;
    private List<Image> ammoList;

    private void Awake()
    {
        ammoCounterTextPro = ammoCounter.GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ammoList = new List<Image>();
    }

    private void generateAmmoImages(int maximumAmmo)
    {
        for(int i = 0; i < maximumAmmo; i++)
        {
            GameObject ammoImage = Instantiate(ammoEnabledPrefab, ammoEnabledParent);
            Instantiate(ammoDisabledPrefab, ammoDisabledParent);
            Image image = ammoImage.GetComponent<Image>();
            if (image != null)
            {
                ammoList.Add(image);
            }
        }
    }

    public void SetAmmo(int currentAmmo, int maxAmmoCount)
    {
        enableSomeAmmo(maxAmmoCount - currentAmmo);
        ammoCounterTextPro.text = $"{currentAmmo}/{maxAmmoCount}";
    }

    public void SetMaximumAmmo(int maxAmmoCount)
    {
        destroyAmmos();
        generateAmmoImages(maxAmmoCount);
        ammoCounterTextPro.text = $"0/{maxAmmoCount}";
    }

    public void ReloadAmmo(int maxAmmoCount)
    {
        enableAllAmmo();
        ammoCounterTextPro.text = $"{maxAmmoCount}/{maxAmmoCount}";
    }

    public void PopAmmo(int currentAmmo, int maxAmmoCount)
    {
        if (ammoList.Count > 0)
        {
            ammoList.ElementAt(maxAmmoCount - currentAmmo - 1).enabled = false;
            ammoCounterTextPro.text = $"{currentAmmo}/{maxAmmoCount}";
        }
    }

    private void destroyAmmos() // NOT BEST WAY!!!! треба зробити, щоб воно одразу генерували штук 100 а ми тільки включали чи відключали їх
    {
        ammoList.Clear();
        for (int i = ammoEnabledParent.childCount - 1; i >= 0; i--)
        {
            GameObject childObject = ammoEnabledParent.GetChild(i).gameObject;

            Destroy(childObject);
        }
        for (int i = ammoDisabledParent.childCount - 1; i >= 0; i--)
        {
            GameObject childObject = ammoDisabledParent.GetChild(i).gameObject;

            Destroy(childObject);
        }
    }

    private void enableSomeAmmo(int ammoCount)
    {
        for(int i = 0; i < ammoCount; i++)
        {
            ammoList.ElementAt(i).enabled = false;
        }
    }

    private void enableAllAmmo()
    {
        foreach(var ammo in ammoList)
        {
            ammo.enabled = true;
        }
    }

    public void ShowHideUI(bool state)
    {
        ammoEnabledParent.gameObject.SetActive(state);
        ammoDisabledParent.gameObject.SetActive(state);
        ammoCounterTextPro.enabled = state;
    }
}
