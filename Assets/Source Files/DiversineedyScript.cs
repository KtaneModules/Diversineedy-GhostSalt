using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Components;

public class DiversineedyScript : MonoBehaviour
{
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMNeedyModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;

    public Aerial Aerial;

    public GameObject ComponentParent;
    private List<ModComponent> Components = new List<ModComponent>();
    private List<Coroutine> ComponentHandlers = new List<Coroutine>();

    private Coroutine BatteryCoroutine;

    private const float StartingDrainRate = 2f;
    private const float EndingDrainRate = 4f;
    private const float StartingChargeRate = 1.5f;
    private const float EndingChargeRate = 1f;

    private float ChargeRemaining;
    private float ChargeDifference;     // Used for the button component.

    private bool IsActive, IsFocused;

    public float LerpBySolves(float start, float end)
    {
        if (Bomb.GetSolvableModuleIDs().Count() < 2)
            return end;
        return Mathf.Lerp(start, end, (Bomb.GetSolvedModuleIDs().Count() * 1f) / (Bomb.GetSolvableModuleIDs().Count() - 1));
    }

    public bool GetIsActive()
    {
        return IsActive;
    }

    public void SetChargeDifference(float value)
    {
        ChargeDifference = value;
    }

    public void ModifyCharge(float value)
    {
        ChargeRemaining += value;
        ChargeRemaining = Mathf.Clamp(ChargeRemaining, 0.01f, 99);
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;

        var possible = ComponentParent.GetComponentsInChildren<ModComponent>().Shuffle().ToArray();
        for (int i = 0; i < 6; i++)
        {
            Components.Add(possible[i]);
            possible[i].transform.localPosition = new Vector3(new[] { -2.5f, 0, 2.5f }[i % 3], possible[i].transform.localPosition.y, new[] { 1, -1 }[i / 3]);
        }
        for (int i = possible.Length - 1; i > 5; i--)
            possible[i].gameObject.SetActive(false);
        Module.OnNeedyActivation += delegate { Activate(); };
        Module.OnNeedyDeactivation += delegate { Deactivate(); };
        Module.OnTimerExpired += delegate { HandleComponentStrike(); };
        Module.GetComponent<KMSelectable>().OnFocus += delegate { IsFocused = true; };
        Module.GetComponent<KMSelectable>().OnDefocus += delegate { IsFocused = false; };
        Bomb.OnBombSolved += delegate { Deactivate(); };
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsFocused && Input.GetKeyDown(KeyCode.Space))
            Aerial.Toggle();
    }

    private void Activate()
    {
        IsActive = true;
        ChargeRemaining = 99f;
        foreach (var comp in Components)
            ComponentHandlers.Add(StartCoroutine(comp.Handle()));
        if (BatteryCoroutine != null)
            StopCoroutine(BatteryCoroutine);
        BatteryCoroutine = StartCoroutine(RunBattery());
    }

    private void Deactivate()
    {
        IsActive = false;
        StopCoroutine(BatteryCoroutine);

        foreach (var handler in ComponentHandlers)
            if (handler != null)
                StopCoroutine(handler);
        ComponentHandlers = new List<Coroutine>();

        foreach (var comp in Components)
            StartCoroutine(comp.Disable());
    }

    private IEnumerator RunBattery()
    {
        while (true)
        {
            if (Aerial.GetIsDown())
                ChargeRemaining += (LerpBySolves(StartingChargeRate, EndingChargeRate) + ChargeDifference) * Time.deltaTime;
            else
                ChargeRemaining -= LerpBySolves(StartingDrainRate, EndingDrainRate) * Time.deltaTime;
            ChargeRemaining = Mathf.Clamp(ChargeRemaining, 0.01f, 99);
            Module.SetNeedyTimeRemaining(ChargeRemaining);
            yield return null;
        }
    }

    public void HandleComponentStrike()
    {
        Module.HandleStrike();
        Module.HandlePass();
        Deactivate();
    }
}
