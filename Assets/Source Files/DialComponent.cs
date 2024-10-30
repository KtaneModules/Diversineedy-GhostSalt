using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
using System.Linq;
using Rnd = UnityEngine.Random;

public class DialComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    private KMAudio.KMAudioRef Sound;

    public KMSelectable Dial;

    private const float StartingDrainRate = 150f;
    private const float EndingDrainRate = 80f;
    private const float StartingWindRate = 1f;
    private const float EndingWindRate = 2f;

    private Coroutine AnimCoroutine;
    private float Progress;

    void Awake()
    {
        Dial.OnInteract += delegate { DialHold(); return false; };
        Dial.OnInteractEnded += delegate { DialRelease(); };

        Bomb.OnBombExploded += delegate { if (Sound != null) Sound.StopSound(); };
    }

    void SetDialPosition(float pos)
    {
        Dial.transform.localEulerAngles = new Vector3(Dial.transform.localEulerAngles.x, Dial.transform.localEulerAngles.y, pos);
    }

    private void DialHold()
    {
        if (!Aerial.GetIsDown() && Main.GetIsActive())
        {
            if (Sound != null)
                Sound.StopSound();
            Sound = Audio.PlaySoundAtTransformWithRef("winding", Dial.transform);
            Dial.AddInteractionPunch();

            if (AnimCoroutine != null)
                StopCoroutine(AnimCoroutine);
            AnimCoroutine = StartCoroutine(Charge());
        }
    }

    private void DialRelease()
    {
        if (!Aerial.GetIsDown() && Main.GetIsActive())
        {
            if (Sound != null)
                Sound.StopSound();

            if (AnimCoroutine != null)
                StopCoroutine(AnimCoroutine);
            AnimCoroutine = StartCoroutine(Drain());
        }
    }

    private IEnumerator Drain()
    {
        while (true)
        {
            Progress += Main.LerpBySolves(360 / StartingDrainRate, 360 / EndingDrainRate) * Time.deltaTime;
            Progress = Mathf.Clamp(Progress, 0, 360);
            SetDialPosition(Progress);
            if (Progress == 360f)
            {
                Main.HandleComponentStrike();
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Charge()
    {
        while (true)
        {
            Progress -= Main.LerpBySolves(360 / StartingWindRate, 360 / EndingWindRate) * Time.deltaTime;
            Progress = Mathf.Clamp(Progress, 0, 360);
            SetDialPosition(Progress);
            if (Progress == 0)
                if (Sound != null)
                    Sound.StopSound();
            yield return null;
        }
    }

    public override IEnumerator Handle()
    {
        if (AnimCoroutine != null)
            StopCoroutine(AnimCoroutine);
        AnimCoroutine = StartCoroutine(Drain());
        yield break;
    }

    public override IEnumerator Disable()
    {
        if (Sound != null)
            Sound.StopSound();
        Sound = Audio.PlaySoundAtTransformWithRef("winding", Dial.transform);

        if (AnimCoroutine != null)
            StopCoroutine(AnimCoroutine);
        AnimCoroutine = StartCoroutine(Charge());

        yield return new WaitUntil(() => Progress == 0);

        if (Sound != null)
            Sound.StopSound();

        if (AnimCoroutine != null)
            StopCoroutine(AnimCoroutine);

        yield break;
    }
}