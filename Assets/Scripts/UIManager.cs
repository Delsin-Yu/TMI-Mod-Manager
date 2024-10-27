using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIManager : MonoBehaviour
{
    public GameObject fileButtonPrefab;
    public Transform fileListContainer;
    public Text jsonContentText;
    public Button deleteButton; // ������ɾ����ť
    public Button refreshButton; // ������ˢ�°�ť
    public GameObject confirmDialog; // ȷ�ϵ���
    public Text confirmDialogText; // ȷ�ϵ����ı�
    public Button confirmButton; // ȷ�ϰ�ť
    public Button cancelButton; // ȡ����ť

    private string filePathToDelete; // ���ڴ洢��ɾ�����ļ�·��
    private string[] filesToIncludeToDelete; // ���ڴ洢��ɾ���Ĺ����ļ�
    private string disableFilePathToDelete; // ���ڴ洢��ɾ���� .DISABLE �ļ�·��

    void Start()
    {
        if (refreshButton == null || deleteButton == null || confirmButton == null || cancelButton == null || confirmDialog == null || confirmDialogText == null)
        {
            Debug.LogError("UIManager: One or more references are not set.");
            return;
        }

        refreshButton.onClick.AddListener(RefreshFileList); // ȷ�������ť������ˢ�¹���
        confirmButton.onClick.AddListener(DeleteConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
        RefreshFileList(); // ��ʼ��ʱˢ��һ���ļ��б�
    }

    public void UpdateUIWithFiles(string[] jsonFiles)
    {
        foreach (string file in jsonFiles)
        {
            CreateFileButton(file);
        }
    }

    void CreateFileButton(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        Toggle toggle = buttonObj.GetComponentInChildren<Toggle>();

        if (buttonText == null || toggle == null)
        {
            Debug.LogError("UIManager: Missing UI components on prefab. Please ensure Text and Toggle are correctly set.");
            return;
        }

        buttonText.text = pluginInfo.pluginName; // ���ð�ť�ı�Ϊ pluginName

        // ���Ŀ¼���Ƿ����ͬ�� .DISABLE �ļ�
        string disableFilePath = filePath + ".DISABLE";
        toggle.isOn = !File.Exists(disableFilePath); // ������� .DISABLE �ļ���Toggle ����Ϊ Off����������Ϊ On

        buttonObj.GetComponent<Button>().onClick.AddListener(() => ShowJsonContent(filePath));

        // ��� Toggle �¼�����
        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                if (File.Exists(disableFilePath))
                {
                    File.Delete(disableFilePath);
                }
            }
            else
            {
                if (!File.Exists(disableFilePath))
                {
                    File.Create(disableFilePath).Close();
                }
            }
        });

        // ���ɾ���ļ���ť�¼�
        deleteButton.onClick.AddListener(() => ShowConfirmDialog(filePath, pluginInfo.fileInclude, disableFilePath));
    }

    void ShowConfirmDialog(string filePath, string[] filesToInclude, string disableFilePath)
    {
        filePathToDelete = filePath;
        filesToIncludeToDelete = filesToInclude;
        disableFilePathToDelete = disableFilePath;
        confirmDialogText.text = "Are you sure you want to delete this file?";
        confirmDialog.SetActive(true);
    }

    public void HideConfirmDialog()
    {
        confirmDialog.SetActive(false);
    }

    public void DeleteConfirmed()
    {
        DeleteJsonFile(filePathToDelete, filesToIncludeToDelete, disableFilePathToDelete);
        HideConfirmDialog();
    }

    void DeleteJsonFile(string jsonFilePath, string[] filesToInclude, string disableFilePath)
    {
        // ɾ�� JSON �ļ�
        if (File.Exists(jsonFilePath))
        {
            File.Delete(jsonFilePath);
        }

        // ɾ���������ļ�
        if (filesToInclude != null)
        {
            foreach (string includedFile in filesToInclude)
            {
                string includedFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), includedFile);
                if (File.Exists(includedFilePath))
                {
                    File.Delete(includedFilePath);
                }
            }
        }

        // ɾ�� .DISABLE �ļ�
        if (File.Exists(disableFilePath))
        {
            File.Delete(disableFilePath);
        }

        // ˢ���ļ��б�
        RefreshFileList();
    }

    public void RefreshFileList()
    {
        // ��յ�ǰ����
        foreach (Transform child in fileListContainer)
        {
            Destroy(child.gameObject);
        }

        // ���¶�ȡ�ļ��б�
        string[] jsonFiles = Directory.GetFiles(Application.dataPath, "*.json");
        UpdateUIWithFiles(jsonFiles);
    }

    void ShowJsonContent(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);
        string includedFiles = pluginInfo.fileInclude != null ? string.Join(", ", pluginInfo.fileInclude) : "None";
        jsonContentText.text = $"Plugin Name: {pluginInfo.pluginName}\nAuthor: {pluginInfo.author}\nVersion: {pluginInfo.version}\nIncluded Files: {includedFiles}";
    }
}
