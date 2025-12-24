using System;
using System.Collections.Generic;
using Assets.Code.Components;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Playables;

public class MainController : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public RenderTexture renderTexture;
    public Card Card;

    [Button]
    public void Run()
    {
        if (Card == null)
        {
            Debug.LogError("Card is null");
            return;
        }
        if (!Card.Prepared)
        {
            Debug.LogError("Card is not prepared");
            return;
        }

        Process();
    }

    private async void Process()
    {
        var settings = new RecorderControllerSettings();
        settings.FrameRate = 60;
        var movie = new MovieRecorderSettings();
        movie.FrameRate = 60;
        movie.EncodingQuality = MovieRecorderSettings.VideoEncodingQuality.High;
        
        var imageInputSettings = new RenderTextureInputSettings
        {
            RenderTexture = renderTexture
        };

        movie.ImageInputSettings = imageInputSettings;

        recorderController = new RecorderController(settings);
        recorderController.Settings.AddRecorderSettings(movie);

        var setters = Card.Setters();

        await Shuffle(setters);
    }

    private async UniTask<bool> Shuffle(List<List<Action>> array, Action[] result = null, int depth = 0)
    {
        if (result == null)
            result = new Action[array.Count];

        if (depth == array.Count)
        {
            foreach (var e in result)
                e.Invoke();
            
            await Record();
            await UniTask.Delay(1000);
            
            return true;
        }

        foreach (var a in array[depth])
        {
            result[depth] = a;
            await Shuffle(array, result, depth + 1);
        }

        return true;
    }

    private async UniTask<bool> Record()
    {
        bool done = false;

        recorderController.PrepareRecording();
        recorderController.StartRecording();

        playableDirector.time = 0;
        playableDirector.Evaluate();
        playableDirector.Play();

        while (!done)
        {
            await UniTask.Yield();

            if (playableDirector.state == PlayState.Paused)
                done = true;
        }

        recorderController.StopRecording();

        return true;
    }

    private RecorderController recorderController;
}
