using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTurn : MonoBehaviour
{

    [Header("위아래 둥실")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("회전")]
    [SerializeField] private float rotateSpeed = 90f; // 초당 회전 각도

    private Vector3 _originPos;

    private void OnEnable()
    {
        _originPos = transform.position;
    }

    private void Update()
    {
        float y = _originPos.y
                + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = new Vector3(
            _originPos.x, y, _originPos.z);

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}

