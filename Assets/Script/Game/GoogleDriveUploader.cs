using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using UnityEngine;
using System.Collections.Generic;

public class GoogleDriveUploader : MonoBehaviour
{
    private static string[] Scopes = { DriveService.Scope.DriveFile };
    private static string ApplicationName = "Your Unity App";

    void Start()
    {
        // ���� ���� Ű ���� ���
        string serviceAccountPath = "Assets/Resources/service_account.json"; // ���� ���� Ű ������ ���

        GoogleCredential credential;
        using (var stream = new System.IO.FileStream(serviceAccountPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        UploadFile(service, "Assets/Resources/highlight.mp4", "1rfGLCSSpFLbriwM8onkiuXPQ_8lLRw8C"); // ���ε��� ������ ��ο� ���� ID
    }

    private void UploadFile(DriveService service, string filePath, string folderId)
    {
        try
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = System.IO.Path.GetFileName(filePath),
                Parents = new List<string> { folderId } // ������ Ư�� ������ ���ε�
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "video/mp4");
                request.Fields = "id";
                IUploadProgress progress = request.Upload();

                if (progress.Status == UploadStatus.Failed)
                {
                    Debug.LogError($"Upload Failed. Error: {progress.Exception.Message}");
                }
            }

            var file = request.ResponseBody;
            if (file != null)
            {
                // ������ ������ ����
                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "anyone",
                    Role = "reader"
                };
                service.Permissions.Create(permission, file.Id).Execute();

                // ���� ������ ���� ����
                var userPermission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "user",
                    Role = "writer",
                    EmailAddress = "jonghwa0212@gmail.com"
                };
                service.Permissions.Create(userPermission, file.Id).Execute();

                string fileUrl = $"https://drive.google.com/uc?id={file.Id}";
                Debug.Log("File URL: " + fileUrl);
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
}