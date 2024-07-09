using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Photon.Pun;
using UnityEngine;

public class GoogleDriveUploader : MonoBehaviourPunCallbacks
{
    private string projectRoot;
    private string videoOutputFolder;
    private static string[] Scopes = { DriveService.Scope.DriveFile };
    private static string ApplicationName = "Your Unity App";
    private DriveService service;
    public string Url { get; set; }
    private static GoogleDriveUploader _instance;
    public static GoogleDriveUploader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GoogleDriveUploader>();

                if (_instance == null)
                {
                    Debug.LogError("GoogleDriveUploader instance not found. Make sure there is one active in the scene.");
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 인스턴스가 파괴되지 않도록 합니다.
        }
    }
    void Start()
    {
        // 서비스 계정 키 파일 경로
        TextAsset jsonAsset = Resources.Load<TextAsset>("service_account"); // Do not include the file extension
        if (jsonAsset == null)
        {
            Debug.LogError("Failed to load service account JSON file from Resources.");
            return;
        }
        GoogleCredential credential;
        using (var stream = new MemoryStream(jsonAsset.bytes))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }
        service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public void UploadGo()
    {
        //return;//TEST 진행 시 주석하고 ㄱ
        projectRoot = Application.dataPath.Replace("/Assets", "");
        videoOutputFolder = Path.Combine(projectRoot, "Videos");
        string videoFilePath = Path.Combine(videoOutputFolder, "highlight.mp4");

        if (!File.Exists(videoFilePath))
        {
            Debug.LogError($"Video file not found at path: {videoFilePath}");
            return;
        }

        UploadFileAsync(service, videoFilePath, "1rfGLCSSpFLbriwM8onkiuXPQ_8lLRw8C");
    }

    private async void UploadFileAsync(DriveService service, string filePath, string folderId)
    {
        try
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderId } // 파일을 특정 폴더에 업로드
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "video/mp4");
                request.Fields = "id";

                var progress = await request.UploadAsync();
                while (progress.Status == UploadStatus.Uploading)
                {
                    Debug.Log($"Upload progress: {progress.BytesSent}");
                    await Task.Delay(500); // 잠시 대기
                }

                if (progress.Status == UploadStatus.Failed)
                {
                    Debug.LogError($"Upload Failed. Error: {progress.Exception.Message}");
                    return;
                }
            }

            var file = request.ResponseBody;
            if (file != null)
            {
                // 파일을 공개로 설정
                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "anyone",
                    Role = "reader"
                };
                await service.Permissions.Create(permission, file.Id).ExecuteAsync();

                // 개인 계정과 파일 공유
                var userPermission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = "jonghwa0212@gmail.com"
                };
                await service.Permissions.Create(userPermission, file.Id).ExecuteAsync();

                string fileUrl = $"https://drive.google.com/uc?id={file.Id}";
                Debug.Log("File URL: " + fileUrl);

                // Call method to handle the file URL, for example:
                HandleFileUrl(fileUrl);
            }
            else
            {
                Debug.LogError("File upload response is null.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during upload: {ex.Message}");
        }
    }

    private void HandleFileUrl(string fileUrl)
    {
        photonView.RPC("BroadcastFileUrl", RpcTarget.All, fileUrl);
    }

    [PunRPC]
    private void BroadcastFileUrl(string fileUrl)
    {
        Url = fileUrl;
        Debug.Log("Broadcasting URL: " + fileUrl);
    }
}