using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneralObjectSOList", menuName = "ScriptableObjects/GeneralObjectSOList", order = 1)]
public class GeneralObjectSOList : ScriptableObject{
   
    public List<Transform> GeneralObjects;

}
