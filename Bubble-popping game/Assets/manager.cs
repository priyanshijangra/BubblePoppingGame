using UnityEngine;
using System.IO;
using Leap;

public class BubbleManager : MonoBehaviour
{
    [Header("Bubbles")]
    [Tooltip("Drag all 10 bubble GameObjects here (in any order).")]
    public GameObject[] bubbles;

    [Header("Audio (optional fallback if a bubble has no AudioSource of its own)")]
    public AudioSource popSound;

    [Header("Behaviour")]
    [Tooltip("If true, the same bubble won't be picked twice in a row.")]
    public bool avoidImmediateRepeat = true;

    [Header("Data Recording")]
    public LeapServiceProvider leapProvider;
    public string fileName = "Participant_01";

    private StreamWriter writer;
    private bool isRecording = false;
    private int trialNumber = 0; // 0 = before the first pop, no recording yet
    private float trialStartTime = 0f;

    private int currentIndex = -1;

    void Start()
    {
        InitializeFile();
        // Make sure everything starts hidden, then activate one at random.
        foreach (GameObject b in bubbles) b.SetActive(false);
        ActivateRandomBubble();
    }

    void Update()
    {
        if (isRecording) RecordHandData();
    }

    void InitializeFile()
    {
        string fullPath = Path.Combine(Application.dataPath, fileName + ".csv");
        writer = new StreamWriter(fullPath, true);
        if (!File.Exists(fullPath) || new FileInfo(fullPath).Length == 0)
        {
            writer.WriteLine("Time,IndexTipX,IndexTipY,IndexTipZ,Trial");
        }
    }

    void RecordHandData()
    {
        if (leapProvider == null || writer == null) return;
        Frame frame = leapProvider.CurrentFrame;
        if (frame == null || frame.Hands.Count == 0) return;

        foreach (Hand hand in frame.Hands)
        {
            if (hand.IsLeft) continue; // recording the right hand only, same as your original script

            Finger indexFinger = hand.fingers[(int)Finger.FingerType.INDEX];
            Vector3 worldTipPos = leapProvider.transform.TransformPoint(indexFinger.TipPosition);

            writer.WriteLine(string.Format("{0:F6},{1:F6},{2:F6},{3:F6},{4}",
                Time.time - trialStartTime, worldTipPos.x, worldTipPos.y, worldTipPos.z, trialNumber));
        }
    }

    // Called by BubbleTrigger when a bubble gets popped.
    public void OnBubblePopped(GameObject poppedBubble)
    {
        // Flush and close out whatever was being recorded for the trial that just ended.
        if (isRecording && writer != null) writer.Flush();

        trialNumber++;          // trial 1 starts now, trial 2 starts on the next pop, etc.
        trialStartTime = Time.time;
        isRecording = true;     // (re)start recording for the new trial

        poppedBubble.SetActive(false);
        ActivateRandomBubble();
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }
    }

    void ActivateRandomBubble()
    {
        if (bubbles == null || bubbles.Length == 0) return;

        int nextIndex;
        if (bubbles.Length == 1)
        {
            nextIndex = 0;
        }
        else
        {
            do
            {
                nextIndex = Random.Range(0, bubbles.Length);
            }
            while (avoidImmediateRepeat && nextIndex == currentIndex);
        }

        currentIndex = nextIndex;
        bubbles[currentIndex].SetActive(true);
    }
}