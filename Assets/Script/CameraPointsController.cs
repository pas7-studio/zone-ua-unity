using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraPointsController : MonoBehaviour
{
    [SerializeField] private List<GameObject> pointsList;

    // Start is called before the first frame update
    void Start()
    {
        pointsList = GameObject.FindGameObjectsWithTag("CameraPoint").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject getPointByName(string name)
    {
        return pointsList.Find(fn =>  fn.GetComponent<CameraPoint>().pointName == name);
    }
}
