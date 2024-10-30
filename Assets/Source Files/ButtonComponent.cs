using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
using System.Linq;
using Rnd = UnityEngine.Random;

public class ButtonComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    private KMAudio.KMAudioRef Sound;

    public KMSelectable Button;

    private const float StartingInterval = 80f;
    private const float EndingInterval = 50f;
    private const float StartingSpeed = 0.0125f;
    private const float EndingSpeed = 0.05f;
    private const float StartingPenalty = 3f;
    private const float EndingPenalty = 10f;
    private const float StartingWarnTime = 1f;
    private const float EndingWarnTime = 2f;
    private const float ButtonDepression = 0.1f;

    private Coroutine ButtonAnimCoroutine;
    private Coroutine DrainCoroutine;
    private float Progress;
    private bool JustReset;

    void Awake()
    {
        Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, 0);

        Button.OnInteract += delegate { ButtonPress(); return false; };

        Bomb.OnBombExploded += delegate { if (Sound != null) Sound.StopSound(); };
    }

    private void ButtonPress()
    {
        if (!Aerial.GetIsDown())
        {
            Audio.PlaySoundAtTransform("big press", Button.transform);
            Button.AddInteractionPunch();
            if (ButtonAnimCoroutine != null)
                StopCoroutine(ButtonAnimCoroutine);
            ButtonAnimCoroutine = StartCoroutine(ButtonAnim());

            if (Main.GetIsActive())
            {
                Audio.PlaySoundAtTransform("shock", Button.transform);
                if (DrainCoroutine != null)
                    StopCoroutine(DrainCoroutine);
                if (Sound != null)
                    Sound.StopSound();
                Main.SetChargeDifference(0);
                Main.ModifyCharge(-Main.LerpBySolves(StartingPenalty, EndingPenalty));
                Progress = 0;
                JustReset = true;
            }
        }
    }

    private IEnumerator Drain()
    {
        float timer = 0;
        while (timer < Main.LerpBySolves(StartingInterval, EndingInterval))
        {
            yield return null;
            timer += Time.deltaTime;
        }
        if (Sound != null)
            Sound.StopSound();
        //Sound = Audio.PlaySoundAtTransformWithRef("alarm quiet", Main.transform);
        bool state = false;
        while (true)
        {
            Progress -= Main.LerpBySolves(StartingSpeed, EndingSpeed) * Time.deltaTime;
            Main.SetChargeDifference(Progress);
            if (!state && Progress < -Main.LerpBySolves(StartingWarnTime, EndingWarnTime))
            {
                if (Sound != null)
                    Sound.StopSound();
                //Sound = Audio.PlaySoundAtTransformWithRef("alarm loud", Main.transform);      // I'm commenting these out, because sound cues ease paranoia. :)
                state = true;
            }
            yield return null;
        }
    }

    private IEnumerator ButtonAnim(float duration = 0.1f)
    {
        Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, 0);

        float timer = 0;
        while (timer < (duration / 2))
        {
            Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, Mathf.Lerp(0, -ButtonDepression, timer / (duration / 2)));
            yield return null;
            timer += Time.deltaTime;
        }

        Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, -ButtonDepression);

        timer = 0;
        while (timer < (duration / 2))
        {
            Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, Mathf.Lerp(-ButtonDepression, 0, timer / (duration / 2)));
            yield return null;
            timer += Time.deltaTime;
        }

        Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, 0);
    }

    public override IEnumerator Handle()
    {
        while (true)
        {
            if (DrainCoroutine != null)
                StopCoroutine(DrainCoroutine);
            DrainCoroutine = StartCoroutine(Drain());
            yield return new WaitUntil(() => JustReset);
            JustReset = false;
        }
    }

    public override IEnumerator Disable()
    {
        if (DrainCoroutine != null)
            StopCoroutine(DrainCoroutine);
        if (Sound != null)
            Sound.StopSound();
        Main.SetChargeDifference(0);
        Progress = 0;
        yield break;
    }
}