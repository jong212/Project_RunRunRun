using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CameraRecorder : MonoBehaviour
{
    public Camera captureCamera;  // Inspector에서 카메라를 할당합니다.
    public RenderTexture renderTexture;  // Inspector에서 RenderTexture를 할당합니다.

    private bool isRecording = false;
    private int frameCount = 0;
    private string projectRoot;
    private string videoOutputFolder;
    private string capturedFramesRootFolder;
    private string currentSessionFolder;
    private string inputFileList;

    private Texture2D reusableTexture;

    private void Start()
    {
        projectRoot = Application.dataPath.Replace("/Assets", "");
        videoOutputFolder = Path.Combine(projectRoot, "Videos");
        capturedFramesRootFolder = Path.Combine(projectRoot, "CapturedFrames");

        if (!Directory.Exists(videoOutputFolder))
        {
            Directory.CreateDirectory(videoOutputFolder);
        }

        if (!Directory.Exists(capturedFramesRootFolder))
        {
            Directory.CreateDirectory(capturedFramesRootFolder);
        }

        // Clear folders at start
        ClearFolder(videoOutputFolder);
        ClearFolder(capturedFramesRootFolder);

        // Initialize inputFileList
        inputFileList = Path.Combine(videoOutputFolder, "input.txt");

        // Initialize reusableTexture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(640, 360, 0, RenderTextureFormat.ARGB32)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                antiAliasing = 1,
                depth = 0,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 0
            };
        }
        reusableTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    }

    private void InitializeInputFile()
    {
        // Clear the input file
        File.WriteAllText(inputFileList, "");
        Debug.Log("Input file initialized");
    }

    private void ClearFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            var directories = Directory.GetDirectories(folderPath);
            foreach (var directory in directories)
            {
                Directory.Delete(directory, true);
            }

            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            Debug.Log($"Cleared folder: {folderPath}");  // Add debug log for folder clearing
        }
    }

    private void CreateNewSessionFolder()
    {
        string sessionID = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        currentSessionFolder = Path.Combine(capturedFramesRootFolder, sessionID);

        if (!Directory.Exists(currentSessionFolder))
        {
            Directory.CreateDirectory(currentSessionFolder);
        }

        Debug.Log("New session folder created: " + currentSessionFolder);
    }

    public void OnRecordButtonClicked()
    {
        if (!isRecording)
        {
            StartRecordingSession();
        }
    }

    void StartRecordingSession()
    {
        CreateNewSessionFolder();
        isRecording = true;
        frameCount = 0; // Reset frame count
        StartCoroutine(CaptureFrames());

        // Automatically stop recording after 6 seconds
        Invoke("StopRecording", 6.0f);
    }

    void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            StopAllCoroutines(); // Stop all coroutines
        }
    }

    IEnumerator CaptureFrames()
    {
        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture.active = renderTexture;
            captureCamera.Render();

            reusableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            reusableTexture.Apply();

            byte[] bytes = reusableTexture.EncodeToPNG();
            string filename = Path.Combine(currentSessionFolder, $"frame_{frameCount:D04}.png");
            File.WriteAllBytes(filename, bytes);
            frameCount++;

            Debug.Log($"Captured frame: {filename}");  // Add debug log for each frame
        }
    }

    public async void CombineSegmentsIntoVideo()
    {
        Debug.Log("Combining segments into video...");
        string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
        string outputFilePath = Path.Combine(videoOutputFolder, "highlight.mp4");

        // Clear the input file before writing new entries
        InitializeInputFile();

        // Write session paths to input.txt
        var directories = Directory.GetDirectories(capturedFramesRootFolder);
        using (StreamWriter writer = new StreamWriter(inputFileList, true))
        {
            foreach (var directory in directories)
            {
                string segmentPath = Path.Combine(directory, "frame_%04d.png");
                writer.WriteLine($"file '{segmentPath}'");
                Debug.Log($"Added to input list: {segmentPath}");  // Add debug log for each segment path
            }
        }

        string quotedInputFileList = $"\"{inputFileList}\"";
        string quotedOutputFilePath = $"\"{outputFilePath}\"";

        Process ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = ffmpegPath;
        ffmpegProcess.StartInfo.Arguments = $"-y -f concat -safe 0 -i {quotedInputFileList} -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p {quotedOutputFilePath}";
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.RedirectStandardOutput = true;
        ffmpegProcess.StartInfo.RedirectStandardError = true;
        ffmpegProcess.StartInfo.CreateNoWindow = true;

        ffmpegProcess.OutputDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.Log(args.Data); };
        ffmpegProcess.ErrorDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.Log(args.Data); };

        await Task.Run(() =>
        {
            ffmpegProcess.Start();
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();
            ffmpegProcess.WaitForExit();
        });

        UnityEngine.Debug.Log("FFmpeg process exited with code: " + ffmpegProcess.ExitCode);
        UnityEngine.Debug.Log("Highlight video created at: " + outputFilePath);
    }
}
