using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
// import lib to play audio from unity
using Unity.Audio;
[Serializable]
public class ResponseModel
{
    public PostModel post;
    public string result;
    public List<WordModel> words;
}
[Serializable]
public class PostModel
{
    public float TotalPersentage;
    public int ayaNumber;
    public int id;
    public string postcode;
    public string realAya;
    public int soraNumber;
    public string userAya;
    public string username;
}
[Serializable]
public class WordModel
{
    public int ayaNumber;
    public int id;
    public int postLog;
    public string postcode;
    public string realWord;
    public int soraNumber;
    public string userWord;
    public string username;
    public float wordPersentage;
}
public class RecordButton : MonoBehaviour
{
    public Button button;
    public Text statusText;
    public Text AyaNumber;
    public Text SoraNumber;
    public Text ResultText;
    public Text MistakeText;

    public AudioSource ifGood;

    public AudioSource Mistake;
    
    public AudioSource ifBad;


    private bool isRecording = false;
    private AudioClip recordedClip;
    private int soraNumber = 1;
    private int ayaNumber = 1;

    void Start()
    {
        UpdatetAya(ayaNumber);
        UpdatetSora(soraNumber);
        button.onClick.AddListener(OnButtonClick);
        string[] devices = Microphone.devices;
        Debug.Log("Available Microphone Devices: " + string.Join(", ", devices));
    }

    void OnButtonClick()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
            StartCoroutine(UploadAudio());
        }
    }

    void StartRecording()
    {
        string microphoneDevice = GetMicrophoneDevice();
        if (microphoneDevice == null)
        {
            Debug.LogError("No microphone device found.");
            return;
        }

        recordedClip = Microphone.Start(microphoneDevice, true, 5, AudioSettings.outputSampleRate);
        isRecording = true;
        UpdateStatusText("Recording...");
    }

    void StopRecording()
    {
        Microphone.End(null);
        isRecording = false;
        UpdateStatusText("Stopped recording.");
    }

    string GetMicrophoneDevice()
    {
        string[] devices = Microphone.devices;
        if (devices.Length > 0)
        {
            return devices[0];
        }
        return null;
    }

    void UpdatetAya(int ayan)
    {
        if (AyaNumber != null)
        {
            AyaNumber.text =  "" + ayan;
        }
    }

    void UpdatetSora(int soran)
    {
        if (SoraNumber != null)
        {
            SoraNumber.text = "" + soran;
        }
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    IEnumerator UploadAudio()
    {
        if (recordedClip == null || recordedClip.samples == 0)
        {
            Debug.LogError("No valid audio recorded!");
            yield break;
        }

        byte[] audioData = WavUtility.SaveToWav(recordedClip);
        string base64Audio = Convert.ToBase64String(audioData);

        WWWForm form = new WWWForm();
        form.AddField("soraNumber", soraNumber);
        form.AddField("ayaNumber", ayaNumber);
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post("https://ai.e3lanotopia.software/compare_verse_unity", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseJson = www.downloadHandler.text;
                ResponseModel response = JsonUtility.FromJson<ResponseModel>(responseJson);
                Debug.Log("Response: " + response.post.TotalPersentage);
                DisplayResults(response);
            }
            else
            {
                ifBad.Play();
                Debug.LogError("Audio upload failed: " + www.error);
            }
        }
    }

    void DisplayResults(ResponseModel response)
    {
        bool allWordsAboveThreshold = response.words.All(word => word.wordPersentage >= 80);

        if (allWordsAboveThreshold)
        {
            ayaNumber++;
            Debug.Log("Good job! Aya number increased to: " + ayaNumber);
            // plas good audio
            ifGood.Play();
            UpdateScreenValues(response);
        }
        else
        {
            // plas bad audio
            Mistake.Play();
            var problematicWords = response.words.Where(word => word.wordPersentage < 80);

            string errorMessage = "هناك بعض الاخطاء :\n";
            foreach (var word in problematicWords)
            {
                errorMessage += $"{word.userWord} خاطئة والصحيح : {word.realWord}.\n";
            }

            Debug.LogError(errorMessage);
            MistakeText.text = errorMessage;
            UpdateScreenValues(response);
        }
    }

    void UpdateScreenValues(ResponseModel response)
    {
        UpdatetAya(response.post.ayaNumber + 1);
        ResultText.text = response.post.TotalPersentage.ToString() + "%";
    }
}
