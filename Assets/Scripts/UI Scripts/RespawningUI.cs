using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespawningUI : MonoBehaviour
{
    public static RespawningUI Instance { get; private set; }

    [SerializeField] private GameObject container;
    [SerializeField] private Animator anim;
    [SerializeField] private Text respawnTimerText;


    private void Awake() {
        Instance = this;

        Hide();
    }
    
    public void Show(){
        gameObject.SetActive(true);

        StartCoroutine(Respawn());
    }

    private void Hide(){
        gameObject.SetActive(false);
    }

    private IEnumerator Respawn(){
        anim.SetTrigger("NewNumber");
        respawnTimerText.text = "3";
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("NewNumber");
        respawnTimerText.text = "2";
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("NewNumber");
        respawnTimerText.text = "1";
        yield return new WaitForSeconds(1f);
        Hide();
    }
}
