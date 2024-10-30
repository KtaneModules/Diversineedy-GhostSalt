using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aerial : MonoBehaviour
{
    public KMAudio Audio;
    public KMSelectable AerialSelectable;

    private bool IsDown;
    private bool IsAnimating;
    private bool IsBuffered;
    private bool JustOpened;

    public GameObject AerialPiece1, AerialPiece2;
    private float AerialPiece1Init, AerialPiece2Init;
    private Coroutine AerialAnimCoroutine;

    public GameObject DoorPiece1, DoorPiece2;
    private float DoorPiece1Init, DoorPiece2Init;
    private Coroutine DoorAnimCoroutine;

    public Aerial(bool isDown)
    {
        IsDown = isDown;
    }

    public bool GetIsDown()
    {
        return IsDown;
    }

    public bool GetJustOpened()
    {
        return JustOpened;
    }

    void Awake()
    {
        AerialPiece1Init = AerialPiece1.transform.localPosition.y;
        AerialPiece2Init = AerialPiece2.transform.localPosition.y;
        DoorPiece1Init = DoorPiece1.transform.localPosition.z;
        DoorPiece2Init = DoorPiece2.transform.localPosition.z;

        AerialSelectable.OnInteract += delegate { Toggle(); return false; };
    }

    public void Toggle()
    {
        if (!IsAnimating)
        {
            if (IsDown)
                PullUpAerial();
            else
                PushDownAerial();
        }
        else if (!IsDown)
            IsBuffered = true;
    }

    public void PushDownAerial()
    {
        IsDown = true;
        Audio.PlaySoundAtTransform("aerial push down", AerialSelectable.transform);
        if (AerialAnimCoroutine != null)
            StopCoroutine(AerialAnimCoroutine);
        AerialAnimCoroutine = StartCoroutine(PushDownAerialAnim());
        if (DoorAnimCoroutine != null)
            StopCoroutine(DoorAnimCoroutine);
        DoorAnimCoroutine = StartCoroutine(CloseDoorAnim());
    }

    public void PullUpAerial()
    {
        IsDown = false;
        Audio.PlaySoundAtTransform("aerial pull up", AerialSelectable.transform);
        if (AerialAnimCoroutine != null)
            StopCoroutine(AerialAnimCoroutine);
        AerialAnimCoroutine = StartCoroutine(PullUpAerialAnim());
        if (DoorAnimCoroutine != null)
            StopCoroutine(DoorAnimCoroutine);
        DoorAnimCoroutine = StartCoroutine(OpenDoorAnim());
    }

    private IEnumerator PushDownAerialAnim(float duration = 0.1f, float piece1Distance = 5f, float piece2Distance = 10f)
    {
        AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, AerialPiece1Init, AerialPiece1.transform.localPosition.z);
        AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, AerialPiece2Init, AerialPiece2.transform.localPosition.z);

        float timer = 0;
        while (timer < duration)
        {
            AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, Easing.InSine(timer, AerialPiece1Init, AerialPiece1Init - piece1Distance, duration), AerialPiece1.transform.localPosition.z);
            AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, Easing.InSine(timer, AerialPiece2Init, AerialPiece2Init - piece2Distance, duration), AerialPiece2.transform.localPosition.z);

            yield return null;
            timer += Time.deltaTime;
        }

        AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, AerialPiece1Init - piece1Distance, AerialPiece1.transform.localPosition.z);
        AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, AerialPiece2Init - piece2Distance, AerialPiece2.transform.localPosition.z);
    }

    private IEnumerator PullUpAerialAnim(float duration = 0.2f, float piece1Distance = 5f, float piece2Distance = 10f)
    {
        AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, AerialPiece1Init - piece1Distance, AerialPiece1.transform.localPosition.z);
        AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, AerialPiece2Init - piece2Distance, AerialPiece2.transform.localPosition.z);

        float timer = 0;
        while (timer < duration)
        {
            AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, Easing.InSine(timer, AerialPiece1Init - piece1Distance, AerialPiece1Init, duration), AerialPiece1.transform.localPosition.z);
            AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, Easing.InSine(timer, AerialPiece2Init - piece2Distance, AerialPiece2Init, duration), AerialPiece2.transform.localPosition.z);

            yield return null;
            timer += Time.deltaTime;
        }

        AerialPiece1.transform.localPosition = new Vector3(AerialPiece1.transform.localPosition.x, AerialPiece1Init, AerialPiece1.transform.localPosition.z);
        AerialPiece2.transform.localPosition = new Vector3(AerialPiece2.transform.localPosition.x, AerialPiece2Init, AerialPiece2.transform.localPosition.z);
    }

    private IEnumerator CloseDoorAnim(float duration = 0.3f, float piece1Distance = 0.0468f, float piece2Distance = 0.0955f)
    {
        IsAnimating = true;
        Audio.PlaySoundAtTransform("door", DoorPiece1.transform);
        DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, DoorPiece1Init);
        DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, DoorPiece2Init);

        float timer = 0;
        while (timer < duration)
        {
            DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, Easing.InSine(timer, DoorPiece1Init, DoorPiece1Init - piece1Distance, duration));
            DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, Easing.InSine(timer, DoorPiece2Init, DoorPiece2Init - piece2Distance, duration));

            yield return null;
            timer += Time.deltaTime;
        }

        DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, DoorPiece1Init - piece1Distance);
        DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, DoorPiece2Init - piece2Distance);

        IsAnimating = false;
    }

    private IEnumerator OpenDoorAnim(float duration = 0.3f, float piece1Distance = 0.0468f, float piece2Distance = 0.0955f)
    {
        IsAnimating = true;
        JustOpened = true;
        Audio.PlaySoundAtTransform("door", DoorPiece1.transform);
        DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, DoorPiece1Init - piece1Distance);
        DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, DoorPiece2Init - piece2Distance);

        float timer = 0;
        while (timer < duration)
        {
            DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, Easing.InSine(timer, DoorPiece1Init - piece1Distance, DoorPiece1Init, duration));
            DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, Easing.InSine(timer, DoorPiece2Init - piece2Distance, DoorPiece2Init, duration));

            yield return null;
            timer += Time.deltaTime;

            JustOpened = false;     // A bit janky / inefficient, but should be fine. Sets this to false after one frame.
        }

        DoorPiece1.transform.localPosition = new Vector3(DoorPiece1.transform.localPosition.x, DoorPiece1.transform.localPosition.y, DoorPiece1Init);
        DoorPiece2.transform.localPosition = new Vector3(DoorPiece2.transform.localPosition.x, DoorPiece2.transform.localPosition.y, DoorPiece2Init);

        IsAnimating = false;
        if (IsBuffered)
        {
            Toggle();
            IsBuffered = false;
        }
    }
}
