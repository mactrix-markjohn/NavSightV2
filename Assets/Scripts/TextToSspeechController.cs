using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Meta.WitAi.TTS.Utilities;

public class TextToSspeechController : MonoBehaviour
{
    // Speaker
    [SerializeField] private TTSSpeaker _speaker;

    [SerializeField] private string _dateId = "[DATE]";
    [SerializeField] private string[] _queuedText;

    // States
    private string _voice;
    private bool _loading;
    private bool _speaking;
    private bool _paused;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Stop click
    public void StopClick() => _speaker.Stop();
    // Pause click
    public void PauseClick()
    {
        if (_speaker.IsPaused)
        {
            _speaker.Resume();
        }
        else
        {
            _speaker.Pause();
        }
    }

   

    // Speak phrase click
    public void SpeakClick(string input)
    {
        // Speak phrase
        string phrase = FormatText(input);
        bool queued = false; //_queueButton != null && _queueButton.isOn;
        bool async = false; // _asyncToggle != null && _asyncToggle.isOn;

        // Speak async
        if (async)
        {
            StartCoroutine(SpeakAsync(phrase, queued));
        }
        // Speak queued
        else if (queued)
        {
            _speaker.SpeakQueued(phrase);
        }
        // Speak
        else
        {
            _speaker.Speak(phrase);
        }

        // Queue additional phrases
        if (_queuedText != null && _queuedText.Length > 0 && queued)
        {
            foreach (var text in _queuedText)
            {
                _speaker.SpeakQueued(FormatText(text));
            }
        }
    }
    // Speak async
    private IEnumerator SpeakAsync(string phrase, bool queued)
    {
        // Queue
        if (queued)
        {
            yield return _speaker.SpeakQueuedAsync(new string[] { phrase });
        }
        // Default
        else
        {
            yield return _speaker.SpeakAsync(phrase);
        }

        // Play complete clip
        //if (_asyncClip != null)
        //{
        //    _speaker.AudioSource.PlayOneShot(_asyncClip);
        //}
    }
    // Format text with current datetime
    private string FormatText(string text)
    {
        string result = text;
        if (result.Contains(_dateId))
        {
            DateTime now = DateTime.UtcNow;
            string dateString = $"{now.ToLongDateString()} at {now.ToLongTimeString()}";
            result = text.Replace(_dateId, dateString);
        }
        return result;
    }
}
