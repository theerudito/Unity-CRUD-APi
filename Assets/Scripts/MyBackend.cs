using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyBackend : MonoBehaviour
{
    [SerializeField]
    //private string urlApi = "http://localhost:5262/api/Player";
    private string urlApi =
        "https://drive.google.com/uc?export=download&id=14KCI9Fgl1FdV1-R3tJIFYVCh51GhqguM";

    [SerializeField]
    private TMP_InputField idInputField;

    [SerializeField]
    private TMP_InputField playerNickNameInputField;

    [SerializeField]
    private TextMeshProUGUI idText;

    [SerializeField]
    private TextMeshProUGUI nickNameText;

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private int separation;

    [SerializeField]
    private Button btnGetOnePlayer;

    [SerializeField]
    private Button btnDeletePlayer;

    [System.Serializable]
    public struct PlayerInformation
    {
        public string nickname;
        public string idPlayer;
    }

    [SerializeField]
    private string idPlayerData;

    [SerializeField]
    private PlayerInformation[] allGames;

    public void DrawUI()
    {
        GameObject canvas = GameObject.Find("CanvasData");

        for (int i = 0; i < allGames.Length; i++)
        {
            GameObject newPanel = Instantiate(panel, canvas.transform);

            idText = newPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            nickNameText = newPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            btnGetOnePlayer = newPanel.transform.GetChild(2).GetComponent<Button>();

            string idPlayer = allGames[i].idPlayer;

            btnGetOnePlayer.onClick.AddListener(() => btnGetOne(idPlayer));

            btnDeletePlayer = newPanel.transform.GetChild(3).GetComponent<Button>();

            btnDeletePlayer.onClick.AddListener(() => btnDELETE(idPlayer));

            idText.text = allGames[i].idPlayer;

            nickNameText.text = allGames[i].nickname;

            newPanel.transform.localPosition = new Vector3(0, -separation * i, 0);
        }
    }

    void Start()
    {
        playerNickNameInputField.GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(GetData());
    }

    public void btnAdd() => StartCoroutine(PostData());

    public void btnGetOne(string idPlayer)
    {
        idPlayerData = idPlayer;
        StartCoroutine(GetOneData(idPlayer));
    }

    public void btnEDIT() => StartCoroutine(PutData());

    public void btnDELETE(string idPlayer)
    {
        StartCoroutine(DeleteData(idPlayer));
    }

    void ResetField()
    {
        idInputField.text = "";
        playerNickNameInputField.text = "";
    }

    IEnumerator GetData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(urlApi))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonData = request.downloadHandler.text;

                    SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(jsonData);

                    allGames = new PlayerInformation[data.Count];

                    for (int i = 0; i < data.Count; i++)
                    {
                        allGames[i].idPlayer = $"ID: {data[i]["idPlayer"]}";
                        allGames[i].nickname = $"NICKNAME: {data[i]["nickname"]}";
                    }
                    DrawUI();
                }
                else
                {
                    Debug.Log("Error en la solicitud: " + request.responseCode);
                }
            }
        }
    }

    IEnumerator PostData()
    {
        PlayerInformation playerData = new PlayerInformation();

        playerData.nickname = playerNickNameInputField.text;

        string jsonBody = JsonUtility.ToJson(playerData);

        UnityWebRequest request = new UnityWebRequest(urlApi, "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                StartCoroutine(GetData());
                Debug.Log("Player added");
                ResetField();
            }
            else
            {
                Debug.Log("Error en la solicitud: " + request.responseCode);
            }
        }
    }

    IEnumerator GetOneData(string idPlayer)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{urlApi}/{idPlayer}"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonData = request.downloadHandler.text;

                    SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(jsonData);

                    idInputField.text = data["idPlayer"];

                    playerNickNameInputField.text = data["nickname"];
                }
                else
                {
                    Debug.Log("Error en la solicitud: " + request.responseCode);
                }
            }
        }
    }

    IEnumerator PutData()
    {
        PlayerInformation playerData = new PlayerInformation();

        playerData.nickname = playerNickNameInputField.text;
        playerData.idPlayer = idInputField.text;

        string jsonBody = JsonUtility.ToJson(playerData);

        UnityWebRequest request = new UnityWebRequest($"{urlApi}/{idPlayerData}", "PUT");

        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                StartCoroutine(GetData());
                Debug.Log("Player edited");
                ResetField();
            }
            else
            {
                Debug.Log("Error en la solicitud: " + request.responseCode);
            }
        }
    }

    IEnumerator DeleteData(string idPlayer)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(urlApi + "/" + idPlayer))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    DrawUI();
                    Debug.Log("Player deleted");
                    ResetField();
                }
                else
                {
                    Debug.Log("Error en la solicitud: " + request.responseCode);
                }
            }
        }
    }
}
