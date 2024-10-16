using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMainMenu : MonoBehaviour
{

    [SerializeField] private Transform headOrientation;
    [SerializeField] private Transform explosionBullet;
    [SerializeField] private Transform gunShotPoint;

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && OptionsUI.Instance.isActive == false){
            Instantiate(explosionBullet, gunShotPoint.position, gunShotPoint.rotation);
        }

        RotateOrientationToMouse();
    }

    private void RotateOrientationToMouse(){
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cursorPosV3 = new Vector3(cursorPos.x, cursorPos.y, 0);
        Vector3 cursorToPlayer = cursorPosV3 - base.transform.position;
        float angle = Mathf.Atan2(cursorToPlayer.y, cursorToPlayer.x) * Mathf.Rad2Deg + 90f;

        headOrientation.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
