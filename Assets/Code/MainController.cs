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
    public void RecordShuffles()
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

        foreach (var component in Card.Components)
        {
            while (component.NextSprite())
            {
                Card.Build();
                if (Card.UniqueView())
                {
                    await Record();
                    await UniTask.Delay(1000);
                }
            }
        }
    }

    [Button]
    public async void RecordThisConfiguration()
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

        await Record();
        await UniTask.Delay(1000);
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
