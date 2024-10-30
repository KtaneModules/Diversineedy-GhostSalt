using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
using System.Linq;
using Rnd = UnityEngine.Random;

public class SwitchComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    public KMSelectable Selectable;
    public MeshRenderer[] PressableParts;
    public MeshRenderer LED;
    public SpriteRenderer Glow;

    private const float StartingProbability = 0.125f;
    private const float EndingProbability = 0.5f;
    private const float SwitchDepression = 0.1f;
    private const float SwitchDarkening = 0.7f;

    private Color PressablePartInitColour;
    private Coroutine SwitchAnimCoroutine;
    private Coroutine SwitchTimerCoroutine;
    private bool IsSwitchDown;

    void Awake()
    {
        PressableParts[0].transform.localPosition = new Vector3(PressableParts[0].transform.localPosition.x, PressableParts[0].transform.localPosition.y, -SwitchDepression);
        PressableParts[1].transform.localPosition = new Vector3(PressableParts[1].transform.localPosition.x, PressableParts[1].transform.localPosition.y, 0);

        PressablePartInitColour = PressableParts[0].material.color;

        PressableParts[0].material.color = PressablePartInitColour * SwitchDarkening;
        PressableParts[1].material.color = PressablePartInitColour;

        SetLEDState(false);

        Selectable.OnInteract += delegate { if (!Aerial.GetIsDown()) SwitchPress(); return false; };
    }

    private void SwitchPress()
    {
        IsSwitchDown = !IsSwitchDown;
        Audio.PlaySoundAtTransform("press", Selectable.transform);
        Selectable.AddInteractionPunch();
        if (SwitchAnimCoroutine != null)
            StopCoroutine(SwitchAnimCoroutine);
        SwitchAnimCoroutine = StartCoroutine(SwitchAnim(IsSwitchDown));

        if (Main.GetIsActive())
        {
            if (IsSwitchDown)
                SwitchTimerCoroutine = StartCoroutine(RunSwitchTimer());
            else
            {
                if (SwitchTimerCoroutine != null)
                    StopCoroutine(SwitchTimerCoroutine);
                SetLEDState(false);
            }
        }
    }

    private void SetLEDState(bool state)
    {
        if (state)
            LED.material.color = Glow.color = new Color(0, 168f / 255, 1, 1);
        else
            LED.material.color = Glow.color = Color.clear;
    }

    private IEnumerator SwitchAnim(bool isSwitchedOn, float duration = 0.1f)
    {
        int upIx = isSwitchedOn ? 0 : 1;
        int downIx = isSwitchedOn ? 1 : 0;
        PressableParts[upIx].transform.localPosition = new Vector3(PressableParts[upIx].transform.localPosition.x, PressableParts[upIx].transform.localPosition.y, -SwitchDepression);
        PressableParts[downIx].transform.localPosition = new Vector3(PressableParts[downIx].transform.localPosition.x, PressableParts[downIx].transform.localPosition.y, 0);

        PressableParts[upIx].material.color = PressablePartInitColour * SwitchDarkening;
        PressableParts[downIx].material.color = PressablePartInitColour;

        float timer = 0;
        while (timer < duration)
        {
            PressableParts[upIx].transform.localPosition = new Vector3(PressableParts[upIx].transform.localPosition.x, PressableParts[upIx].transform.localPosition.y, Mathf.Lerp(-SwitchDepression, 0, timer / (duration / 2)));
            PressableParts[downIx].transform.localPosition = new Vector3(PressableParts[downIx].transform.localPosition.x, PressableParts[downIx].transform.localPosition.y, Mathf.Lerp(0, -SwitchDepression, timer / (duration / 2)));

            PressableParts[upIx].material.color = Color.Lerp(PressablePartInitColour * SwitchDarkening, PressablePartInitColour, timer / duration);
            PressableParts[downIx].material.color = Color.Lerp(PressablePartInitColour, PressablePartInitColour * SwitchDarkening, timer / duration);

            yield return null;
            timer += Time.deltaTime;
        }

        PressableParts[upIx].transform.localPosition = new Vector3(PressableParts[upIx].transform.localPosition.x, PressableParts[upIx].transform.localPosition.y, 0);
        PressableParts[downIx].transform.localPosition = new Vector3(PressableParts[downIx].transform.localPosition.x, PressableParts[downIx].transform.localPosition.y, -SwitchDepression);

        PressableParts[upIx].material.color = PressablePartInitColour;
        PressableParts[downIx].material.color = PressablePartInitColour * SwitchDarkening;
    }

    private IEnumerator RunSwitchTimer(float flashSpeed = 0.125f)
    {
        bool state = false;
        float prevPoint = 0;
        float timer = 0;
        while (timer < 5)
        {
            if (timer >= prevPoint + flashSpeed)
            {
                SetLEDState(state);
                prevPoint += flashSpeed;
                state = !state;
            }
            yield return null;
            timer += Time.deltaTime;
        }
        Main.HandleComponentStrike();
    }

    public override IEnumerator Handle()
    {
        if (IsSwitchDown)
        {
            IsSwitchDown = true;
            PressableParts[0].transform.localPosition = new Vector3(PressableParts[0].transform.localPosition.x, PressableParts[0].transform.localPosition.y, 0);
            PressableParts[1].transform.localPosition = new Vector3(PressableParts[1].transform.localPosition.x, PressableParts[1].transform.localPosition.y, -SwitchDepression);

            PressableParts[0].material.color = PressablePartInitColour;
            PressableParts[1].material.color = PressablePartInitColour * SwitchDarkening;

            SwitchTimerCoroutine = StartCoroutine(RunSwitchTimer());
        }
        while (true)
        {
            if (Aerial.GetJustOpened() && !IsSwitchDown && Rnd.Range(0, 1f) <= Main.LerpBySolves(StartingProbability, EndingProbability))
            {
                IsSwitchDown = true;
                PressableParts[0].transform.localPosition = new Vector3(PressableParts[0].transform.localPosition.x, PressableParts[0].transform.localPosition.y, 0);
                PressableParts[1].transform.localPosition = new Vector3(PressableParts[1].transform.localPosition.x, PressableParts[1].transform.localPosition.y, -SwitchDepression);

                PressableParts[0].material.color = PressablePartInitColour;
                PressableParts[1].material.color = PressablePartInitColour * SwitchDarkening;

                SwitchTimerCoroutine = StartCoroutine(RunSwitchTimer());
            }
            yield return null;
        }
    }

    public override IEnumerator Disable()
    {
        if (IsSwitchDown)
            SwitchPress();
        if (SwitchTimerCoroutine != null)
            StopCoroutine(SwitchTimerCoroutine);
        SetLEDState(false);
        yield break;
    }
}