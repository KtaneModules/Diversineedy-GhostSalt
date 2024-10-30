using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
using System.Linq;
using Rnd = UnityEngine.Random;

public class MeterComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    public Transform Liquid;

    private const float StartingDrainRate = 100f;
    private const float EndingDrainRate = 40f;
    private const float StartingDischargeRate = 0.5f;
    private const float EndingDischargeRate = 5f;

    private float Progress;

    private Coroutine LiquidAnimCoroutine;
    private Coroutine ListenerAnimCoroutine;

    private float GetLiquidHeight(float t)
    {
        return Easing.OutSine(t, 0.025f, 1f, 1);
    }

    void Awake()
    {
        Liquid.localScale = new Vector3(Liquid.localScale.x, GetLiquidHeight(0), Liquid.localScale.x);
    }

    private IEnumerator AerialListener()
    {
        bool prevState = Aerial.GetIsDown();
        while (true)
        {
            yield return new WaitUntil(() => Aerial.GetIsDown() != prevState);
            if (Aerial.GetIsDown())
            {
                if (LiquidAnimCoroutine != null)
                    StopCoroutine(LiquidAnimCoroutine);
                LiquidAnimCoroutine = StartCoroutine(LiquidDrain());
            }
            else
            {
                if (LiquidAnimCoroutine != null)
                    StopCoroutine(LiquidAnimCoroutine);
                LiquidAnimCoroutine = StartCoroutine(LiquidDischarge());
            }
            prevState = Aerial.GetIsDown();
        }
    }

    private IEnumerator LiquidDrain()
    {
        while (true)
        {
            Progress += (100 / Main.LerpBySolves(StartingDrainRate, EndingDrainRate)) * Time.deltaTime;
            Progress = Mathf.Clamp(Progress, 0, 100);
            Liquid.localScale = new Vector3(Liquid.localScale.x, GetLiquidHeight(Progress / 100), Liquid.localScale.x);
            if (Progress == 100)
            {
                Main.HandleComponentStrike();
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator LiquidDischarge()
    {
        while (true)
        {
            Progress -= (100 / Main.LerpBySolves(StartingDischargeRate, EndingDischargeRate)) * Time.deltaTime;
            Progress = Mathf.Clamp(Progress, 0, 100);
            Liquid.localScale = new Vector3(Liquid.localScale.x, GetLiquidHeight(Progress / 100), Liquid.localScale.x);
            yield return null;
        }
    }

    public override IEnumerator Handle()
    {
        if (Aerial.GetIsDown())
        {
            if (LiquidAnimCoroutine != null)
                StopCoroutine(LiquidAnimCoroutine);
            LiquidAnimCoroutine = StartCoroutine(LiquidDrain());
        }
        else
        {
            if (LiquidAnimCoroutine != null)
                StopCoroutine(LiquidAnimCoroutine);
            LiquidAnimCoroutine = StartCoroutine(LiquidDischarge());
        }
        if (ListenerAnimCoroutine != null)
            StopCoroutine(ListenerAnimCoroutine);
        ListenerAnimCoroutine = StartCoroutine(AerialListener());
        yield break;
    }

    public override IEnumerator Disable()
    {
        if (LiquidAnimCoroutine != null)
            StopCoroutine(LiquidAnimCoroutine);
        LiquidAnimCoroutine = StartCoroutine(LiquidDischarge());
        if (ListenerAnimCoroutine != null)
            StopCoroutine(ListenerAnimCoroutine);
        yield break;
    }
}