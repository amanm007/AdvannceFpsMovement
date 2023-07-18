using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
    [Header("Jetpack")]
    public KeyCode jetpackKey = KeyCode.LeftShift;
    public float jetpackForce;
    public float jetpackFuelCapacity;
    public float jetpackFuelConsumptionRate;
    public float jetpackRechargeRate;
    private float currentJetpackFuel;

    private Rigidbody rb;
    private bool isJetpackActive;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentJetpackFuel = jetpackFuelCapacity;
    }

    private void Update()
    {
        isJetpackActive = Input.GetKey(jetpackKey);
    }

    private void FixedUpdate()
    {
        HandleJetpack();
        RechargeJetpackFuel();
    }

    private void HandleJetpack()
    {
        if (isJetpackActive && currentJetpackFuel > 0f)
        {
            rb.AddForce(transform.up * jetpackForce, ForceMode.Force);
            currentJetpackFuel -= jetpackFuelConsumptionRate * Time.deltaTime;

            if (currentJetpackFuel <= 0f)
            {
                currentJetpackFuel = 0f;
            }
        }
    }

    private void RechargeJetpackFuel()
    {
        if (!isJetpackActive && currentJetpackFuel < jetpackFuelCapacity)
        {
            currentJetpackFuel += jetpackRechargeRate * Time.deltaTime;

            if (currentJetpackFuel > jetpackFuelCapacity)
            {
                currentJetpackFuel = jetpackFuelCapacity;
            }
        }
    }
    public bool IsJetpackActive()
    {
        return isJetpackActive;
    }
}
