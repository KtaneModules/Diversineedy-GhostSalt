using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;

public class NeedleComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    public GameObject Needle;
    public KMSelectable LeftButton;
    public KMSelectable RightButton;

    private float NeedlePos;
    private const float StrikeRange = 70f;

    private Coroutine LeftButtonAnimCoroutine;
    private Coroutine RightButtonAnimCoroutine;

    private const float StartingRate = 150f;
    private const float EndingRate = 70f;

    private bool IsMovingRight;

    void Awake()
    {
        LeftButton.OnInteract += delegate { LeftButtonPress(); return false; };
        RightButton.OnInteract += delegate { RightButtonPress(); return false; };
        SetNeedlePos(0);
    }

    private void LeftButtonPress()
    {
        if (!Aerial.GetIsDown())
        {
            IsMovingRight = false;
            Audio.PlaySoundAtTransform("press", LeftButton.transform);
            LeftButton.AddInteractionPunch();
            if (LeftButtonAnimCoroutine != null)
                StopCoroutine(LeftButtonAnimCoroutine);
            LeftButtonAnimCoroutine = StartCoroutine(ButtonAnim(LeftButton.transform));
        }
    }

    private void RightButtonPress()
    {
        if (!Aerial.GetIsDown())
        {
            IsMovingRight = true;
            Audio.PlaySoundAtTransform("press", RightButton.transform);
            RightButton.AddInteractionPunch();
            if (RightButtonAnimCoroutine != null)
                StopCoroutine(RightButtonAnimCoroutine);
            RightButtonAnimCoroutine = StartCoroutine(ButtonAnim(RightButton.transform));
        }
    }

    private IEnumerator ButtonAnim(Transform target, float duration = 0.075f, float depression = 0.002f)
    {
        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, 0);
        float timer = 0;
        while (timer < duration / 2)
        {
            target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, Mathf.Lerp(0, -depression, timer / (duration / 2)));
            yield return null;
            timer += Time.deltaTime;
        }

        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, -depression);
        timer = 0;
        while (timer < duration / 2)
        {
            target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, Mathf.Lerp(-depression, 0, timer / (duration / 2)));
            yield return null;
            timer += Time.deltaTime;
        }

        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, 0);
    }

    public override IEnumerator Handle()
    {
        SetNeedlePos(0);

        while (true)
        {
            var rate = 1 / Main.LerpBySolves(StartingRate, EndingRate);
            if (IsMovingRight)
                SetNeedlePos(NeedlePos + (rate * Time.deltaTime));
            else
                SetNeedlePos(NeedlePos - (rate * Time.deltaTime));
            yield return null;
        }
    }

    public override IEnumerator Disable()
    {
        var init = NeedlePos;
        float duration = Mathf.Abs(init / 2);

        float timer = 0;
        while (timer < duration)
        {
            SetNeedlePos(Easing.OutSine(timer, init, 0, duration), false);
            yield return null;
            timer += Time.deltaTime;
        }
        SetNeedlePos(0);
    }

    private void SetNeedlePos(float pos, bool clock = true)
    {
        Needle.transform.localEulerAngles = new Vector3(Needle.transform.localEulerAngles.x, Needle.transform.localEulerAngles.y, Easing.InOutSine(pos + 1, -StrikeRange, StrikeRange, 2));
        if (clock)
        {
            NeedlePos = pos;
            if (Mathf.Abs(NeedlePos) >= 1)
                Main.HandleComponentStrike();
        }
    }
}