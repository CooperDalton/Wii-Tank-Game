using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracksVisual : MonoBehaviour
{
    [SerializeField] private TrailRenderer leftTrail;
    [SerializeField] private TrailRenderer rihtTrail;
    [SerializeField] private float timeOn;
    [SerializeField] private float timeOff;
    private float timer;


    private void Start() {
        timer = timeOn + timeOff;
    }

    private void Update() {
        timer -= Time.deltaTime;

        if (timer > timeOff){
            SetTrailsActive(true);
        } else if (timer < timeOff && timer > 0f){
            SetTrailsActive(false);
        } else{
            timer = timeOn + timeOff;
        }

    }

    public void SetTrailsActive(bool active)
    {
        leftTrail.emitting = active;
        rihtTrail.emitting = active;
    }
}
