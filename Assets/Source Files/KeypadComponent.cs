using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
using System.Linq;
using Rnd = UnityEngine.Random;

public class KeypadComponent : ModComponent
{
    public DiversineedyScript Main;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public Aerial Aerial;

    public KMSelectable[] Keys;
    public TextMesh[] Labels;
    public MeshRenderer[] LEDs;

    private const float StartingSpeed = 140f;
    private const float EndingSpeed = 40f;
    private const float SlowSpeed = 0.5f;

    private static readonly int[] NumberSet = new int[] { 4, 8, 15, 16, 23, 42 };
    private List<int> Numbers = new List<int>();
    private List<int> ChosenKeys = new List<int>();
    private List<int> SelectedKeys = new List<int>();
    private Coroutine KeyTimerCoroutine;
    private Coroutine[] KeyAnimCoroutines;

    void Awake()
    {
        KeyAnimCoroutines = new Coroutine[Keys.Length];

        Numbers = NumberSet.Shuffle().Take(4).ToList();

        for (int i = 0; i < Keys.Length; i++)
        {
            int x = i;
            Keys[x].OnInteract += delegate { KeyPress(x); return false; };
            Labels[x].text = Numbers[i].ToString();
            LEDs[x].material.color = Color.black;
        }
    }

    private void KeyPress(int pos)
    {
        if (!Aerial.GetIsDown())
        {
            Audio.PlaySoundAtTransform("press", Keys[pos].transform);
            Keys[pos].AddInteractionPunch();
            if (KeyAnimCoroutines[pos] != null)
                StopCoroutine(KeyAnimCoroutines[pos]);
            KeyAnimCoroutines[pos] = StartCoroutine(KeyAnim(Keys[pos].transform));

            if (Main.GetIsActive())
            {
                if (!ChosenKeys.Contains(pos) || SelectedKeys.Contains(pos) || ChosenKeys.Any(x => !SelectedKeys.Contains(x) && Numbers[x] < Numbers[pos]))
                    Main.HandleComponentStrike();
                else
                {
                    SelectedKeys.Add(pos);
                    LEDs[pos].material.color = Color.black;

                    if (SelectedKeys.Count() == ChosenKeys.Count())
                    {
                        if (KeyTimerCoroutine != null)
                            StopCoroutine(KeyTimerCoroutine);
                        StartNewSet();
                    }
                }
            }
        }
    }

    private IEnumerator KeyAnim(Transform target, float duration = 0.1f, float depression = 0.1f)
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

    private IEnumerator RunKeyTimer(int[] targets, float flicker = 0.05f, float flashSpeed = 0.125f)
    {
        foreach (var target in targets.Where(x => !SelectedKeys.Contains(x)))
            LEDs[target].material.color = Color.black;
        float duration = Main.LerpBySolves(StartingSpeed, EndingSpeed) / 2;
        float timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime * (Aerial.GetIsDown() ? SlowSpeed : 1);
        }
        timer = 0;
        while (timer < duration)
        {
            foreach (var target in targets.Where(x => !SelectedKeys.Contains(x)))
                LEDs[target].material.color = new Color(Mathf.Min(Mathf.Lerp(0, 1, timer / duration) + Rnd.Range(-flicker, flicker), 1), 0, 0);
            yield return null;
            timer += Time.deltaTime * (Aerial.GetIsDown() ? SlowSpeed : 1);
        }
        bool state = false;
        float prevPoint = 0;
        timer = 0;
        while (timer < 15)
        {
            if (timer >= prevPoint + flashSpeed)
            {
                if (state)
                    foreach (var target in targets.Where(x => !SelectedKeys.Contains(x)))
                        LEDs[target].material.color = Color.red;
                else
                    foreach (var target in targets.Where(x => !SelectedKeys.Contains(x)))
                        LEDs[target].material.color = Color.black;
                prevPoint += flashSpeed;
                state = !state;
            }
            yield return null;
            timer += Time.deltaTime * (Aerial.GetIsDown() ? SlowSpeed : 1);
        }
        Main.HandleComponentStrike();
    }

    public override IEnumerator Handle()
    {
        StartNewSet();
        yield break;
    }

    void StartNewSet()
    {
        ChosenKeys = Enumerable.Range(0, Keys.Length).ToArray().Shuffle().Take(Rnd.Range(2, 4)).ToList();
        SelectedKeys = new List<int>();
        KeyTimerCoroutine = StartCoroutine(RunKeyTimer(ChosenKeys.ToArray()));
    }

    public override IEnumerator Disable()
    {
        ChosenKeys = new List<int>();
        SelectedKeys = new List<int>();
        if (KeyTimerCoroutine != null)
            StopCoroutine(KeyTimerCoroutine);
        for (int i = 0; i < Keys.Length; i++)
            LEDs[i].material.color = Color.black;
        yield break;
    }
}