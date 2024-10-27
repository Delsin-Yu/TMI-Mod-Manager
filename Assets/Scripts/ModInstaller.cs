using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SFB; // ʹ�� StandaloneFileBrowser �����ռ�

public class ModInstaller : MonoBehaviour
{
    public string dirPath; // Ŀ��Ŀ¼·��
    public Button selectFileButton;
    public GameObject confirmDialog; // ȷ�ϵ���
    public Text confirmDialogText; // ȷ�ϵ����ı�
    public Button confirmButton; // ȷ�ϰ�ť
    public Button cancelButton; // ȡ����ť

    public UIManager uiManager;

    private string selectedFilePath;
    private string pluginName;
    private string version;

    void Start()
    {
        selectFileButton.onClick.AddListener(OpenFileSelector);
        confirmButton.onClick.AddListener(InstallConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
    }

    void OpenFileSelector()
    {
        // ʹ�� StandaloneFileBrowser ���ļ�ѡ��Ի���
        var extensions = new[] {
            new ExtensionFilter( "Izakaya File", "izakaya" ),
            new ExtensionFilter( "ZIP File", "zip" ),
            new ExtensionFilter( "All Files", "*" )
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", extensions, true);

        if (paths.Length > 0)
        {
            selectedFilePath = paths[0];
            if (ValidateZipContents(selectedFilePath))
            {
                ShowConfirmDialog();
            }
            else
            {
                Debug.LogError("Zip file validation failed.");
            }
        }
    }

    bool ValidateZipContents(string zipFilePath)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                // ����Ƿ���� Manifest.json �ļ�
                var manifestEntry = archive.GetEntry("Manifest.json");
                if (manifestEntry == null)
                {
                    Debug.LogError("Manifest.json file not found in the archive.");
                    return false;
                }

                // ��ȡ Manifest.json �ļ�����
                using (var reader = new StreamReader(manifestEntry.Open()))
                {
                    string manifestContent = reader.ReadToEnd();
                    // ���� JSON ����
                    var manifest = JsonUtility.FromJson<PluginInfo>(manifestContent);
                    pluginName = manifest.pluginName;
                    version = manifest.version;

                    // ��֤ JSON �����Ƿ�ǿ�
                    if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(version))
                    {
                        Debug.LogError("Manifest.json file is missing required fields.");
                        return false;
                    }
                }

                // У���߼�ʾ����ȷ�������ļ�������
                return archive.Entries.Any(entry => !string.IsNullOrEmpty(entry.Name));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error validating zip contents: " + ex.Message);
            return false;
        }
    }

    void ShowConfirmDialog()
    {
        confirmDialogText.text = $"{pluginName} ({version})";
        confirmDialog.SetActive(true);
    }

    void HideConfirmDialog()
    {
        confirmDialog.SetActive(false);
    }

    void InstallConfirmed()
    {
        try
        {
            ZipFile.ExtractToDirectory(selectedFilePath, dirPath);

            string manifestPath = Path.Combine(dirPath, "Manifest.json");
            if (File.Exists(manifestPath))
            {
                string newManifestPath = Path.Combine(dirPath, $"{pluginName}.json");
                File.Move(manifestPath, newManifestPath);
                Debug.Log($"Manifest.json file renamed to {pluginName}.json");
            }
            uiManager.RefreshFileList();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error installing files: " + ex.Message);
        }
        HideConfirmDialog();
    }
}
