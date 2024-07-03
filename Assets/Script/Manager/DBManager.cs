using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Photon.Pun;
using System.Collections.Generic;


public class DBManager : MonoBehaviour
{

    public NetworkManager networkManager;
    [Header("UI")]

    // �α��� Ui
    [SerializeField] InputField Input_Id;
    [SerializeField] InputField Input_Pw;
    [SerializeField] Text Input_CheckIdPw_Error;
    [SerializeField] Text Text_DBResult;
    [SerializeField] Text Text_Log;

    // ȸ������ Ui
    [SerializeField] GameObject JoinUi;
    [SerializeField] InputField Input_JoinId;

    [SerializeField] InputField Input_JoinPw;
    [SerializeField] InputField Input_JoinPwChk;
    [SerializeField] Text Input_JoinIdMessage;
    [SerializeField] Text Input_JoinIdMessage2;
    [SerializeField] GameObject Btn_confirm;

    [Header("CommectionInfo")]
    string _ip = "43.203.127.106"; // Ensure this is your server's IP
    string _dbName = "test";
    string _uid = "root";
    string _pwd = "1q2w3e4r!";
    string _port = "3306";
    string currentPrafab;
    string nickname;
    public string CurrentPrafab { get => currentPrafab; set => currentPrafab = value; }
    public int CurrentGold { get; set; }
    public string Nickname { get => nickname; set => nickname = value; }
    
    private string _getId = "SELECT * FROM u_info where Nickname =";
    public string GetIdQuery { get => _getId; }
    private bool _idchk;
    public bool _IdChk { get => _idchk; set => _idchk = value; }

    private bool _isConnectTestComplete; //�߿����� ����
    private static MySqlConnection _dbConnection;
    private string SendQuery(string queryStr, string tableName)
    {
        //����� ���� �������� SELECT�� ���ԵǾ� ������ if Ž
        if (queryStr.Contains("SELECT"))
        {
            DataSet dataSet = OnSelectRequest(queryStr, tableName);
            return dataSet != null ? DeformatResult(dataSet) : string.Empty;

        }

        return string.Empty;



    }
    public static bool OnInsertOnUpdateRequest(string query)
    {
        try
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.Connection = _dbConnection;
            sqlCommand.CommandText = query;

            _dbConnection.Open();
            sqlCommand.ExecuteNonQuery();
            _dbConnection.Close();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    private void Update()
    {
        //Debug.Log("currentvalue :::" + CurrentGold);
    }
    private void Awake()
    {
        _isConnectTestComplete = ConnectTest();
        //this.gameObject.SetActive(false);

    }

    private string DeformatResult(DataSet dataSet)
    {
        string resultStr = string.Empty;
        foreach (DataTable table in dataSet.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    resultStr += $"{column.ColumnName} : {row[column]}\n";
                }
            }
        }
        Debug.Log(resultStr);
        return resultStr;
    }
    public static DataSet OnSelectRequest(string query, string tableName)
    {
        try
        {
            _dbConnection.Open();
            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = _dbConnection;
            sqlCmd.CommandText = query;
            MySqlDataAdapter sd = new MySqlDataAdapter(sqlCmd);
            DataSet dataSet = new DataSet();
            sd.Fill(dataSet, tableName);
            _dbConnection.Close();
            return dataSet;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }
    bool ConnectTest()
    {
        string connectStr = $"Server={_ip};Database={_dbName};Uid={_uid};Pwd={_pwd};Port={_port};";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectStr))
            {
                _dbConnection = conn;
                conn.Open();
            }
            Text_Log.text = "����";
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"e: {e.ToString()}");
            //Text_Log.text = "DB���� ����";
            Debug.LogWarning("[1.��� ���� ����]");
            return false;
        }
    }




    // �α��� ��ư  
    public void OnSubmit_Login()
    {
        string query = string.Empty;
        if (_isConnectTestComplete == false)
        {
            Text_Log.text = "DB ������ ���� �õ����ּ���";
            return;
        }

        Text_Log.text = string.Empty;
        if (string.IsNullOrWhiteSpace(Input_Id.text) || string.IsNullOrWhiteSpace(Input_Pw.text))
        {
            Input_CheckIdPw_Error.text = "���̵�� ��й�ȣ�� �Է��� �ּ���.";
            return;
        }
        else
        {
            query = $"SELECT Password FROM u_info WHERE Nickname = '{Input_Id.text}'";
        }

        string result = SendQuery(query, "u_info");

        if (string.IsNullOrEmpty(result))
        {
            Input_CheckIdPw_Error.text = "ID�� �������� �ʽ��ϴ�.";
            return;
        }

        string retrievedPassword = ExtractPassword(result);

        if (retrievedPassword == Input_Pw.text)
        {

            Input_CheckIdPw_Error.text = "�α��� ����!";
            //���⿡ ��Ʈ��ũ �Ŵ��� connect �Լ��� ȣ���ϰ� �;�
            Nickname = Input_Id.text;

            //����ڰ� ������ ĳ���͵� ����Ʈ�� ����
            List<string> temp = GetMycharacter(Nickname);
            foreach (string characterType in temp)
            {
                networkManager.OwnedCharacters.Add(characterType);
            }
            CurrentPrafab = GetCharacterId(Nickname);
            CurrentGold = GetPlayerGold(Nickname);
            networkManager.Connect(CurrentPrafab, Nickname, CurrentGold);

            Input_Id.text = "";
            Input_Pw.text = "";
        }
        else
        {
            Input_CheckIdPw_Error.text = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
        }
    }

    // �α��� ��ư - ��й�ȣ �� �� ��������
    private string ExtractPassword(string result)
    {
        string[] lines = result.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("Password : "))
            {
                return line.Substring("Password : ".Length).Trim();
            }
        }
        return string.Empty;
    }

    public void OnSubmit_Join_idCheck()
    {
        string query = string.Empty;
        if (_isConnectTestComplete == false)
        {
            Text_Log.text = "DB ������ ���� �õ����ּ���";
            return;
        }
        if (string.IsNullOrWhiteSpace(Input_JoinId.text))
        {
            Input_JoinIdMessage.text = "���̵� �Է��� �ּ���";
        }
        else
        {
            query = $"SELECT Password FROM u_info WHERE Nickname = '{Input_JoinId.text}'";
            string result = SendQuery(query, "u_info");

            if (string.IsNullOrEmpty(result))
            {
                Input_JoinIdMessage.text = "��� ����";
                _IdChk = true;
                return;
            }
            else
            {
                Input_JoinIdMessage.text = "������� ���̵�";
                _IdChk = false;
            }
        }
    }
    public string SelectPlayercharacterNumber(string playerId)
    {
        string query = string.Empty;
        string result = string.Empty;
        if (playerId != null)
        {
            query = $"SELECT CharacterNumber FROM u_info WHERE Nickname = '{playerId}'";
            result = SendQuery(query, "u_info");
        }
        else
        {
            Debug.Log("�÷��̾� ���̵� ����");
        }
        return result;
    }
    //ȸ������ �Ϸ� �˾�
    public void OnSubmit_Join_success()
    {
        if (!_IdChk)
        {
            Input_JoinIdMessage2.text = "���̵� �ߺ�üũ �ʼ�";
        }
        else if (string.IsNullOrEmpty(Input_JoinPw.text))
        {
            Input_JoinIdMessage2.text = "��й�ȣ�� �Է����ּ���";
        }
        else if (Input_JoinPw.text != Input_JoinPwChk.text)
        {
            Input_JoinIdMessage2.text = "��й�ȣ�� ���� �ٸ��� �Է���";
        }
        else
        {
            string query = $"INSERT INTO u_info (Nickname, Password) VALUES ('{Input_JoinId.text}', '{Input_JoinPw.text}')";
            nickname = Input_JoinId.text;
            bool isSuccess = OnInsertOnUpdateRequest(query);
            InsertCharacterInfo("Player");


            Btn_confirm.SetActive(!Btn_confirm.activeSelf);
        }
    }


    public void OnSubmit_Join_success_btn()
    {
        JoinUi.SetActive(!JoinUi.activeSelf);
        Btn_confirm.SetActive(!Btn_confirm.activeSelf);

    }
    public void OnClick_JoinUi_Exit()
    {
        JoinUi.SetActive(!JoinUi.activeSelf);

    }
    public void OnClick_OpenDatabaseUI()
    {
        this.gameObject.SetActive(true);
    }

    public void OnClick_CloseDatabaseUI()
    {
        this.gameObject.SetActive(false);
    }
    public List<string> GetMycharacter(string name)
    {
        List<string> tempCurrentCharacter = new List<string>();
        string query = $"SELECT CharacterType FROM character_info WHERE Nickname = '{name}'";
        DataSet dataSet = OnSelectRequest(query, "character_info");
        if (dataSet != null && dataSet.Tables["character_info"].Rows.Count > 0)
        {
            foreach (DataRow row in dataSet.Tables["character_info"].Rows)
            {
                tempCurrentCharacter.Add(row["CharacterType"].ToString());
            }

        }
        return tempCurrentCharacter;

    }
    // �÷��̾� ĳ���� ���̵� ��������
    public string GetCharacterId(string playerId)
    {
        string query = $"SELECT CharacterId FROM u_info WHERE Nickname = '{playerId}'";
        string result = SendQuery(query, "u_info");

        // Assuming the format you need to split is 'key:value', and you need the 'value' part
        if (!string.IsNullOrEmpty(result))
        {
            string[] parts = result.Split(':');
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
        }

        return string.Empty;
    }
    // �÷��̾� ��� ��������

    public int GetPlayerGold(string nickname)
    {
        string query = $"SELECT Money FROM u_info WHERE Nickname = '{nickname}'";
        DataSet dataSet = OnSelectRequest(query, "u_info");

        if (dataSet != null && dataSet.Tables["u_info"].Rows.Count > 0)
        {
            // Get the first row
            DataRow row = dataSet.Tables["u_info"].Rows[0];

            // Retrieve the "Money" column value
            int money = Convert.ToInt32(row["Money"]);

            return money;
        }
        // Return 0 if no data was found
        return 0;
    }

    public void InsertCharacterInfo( string characterType)
    {
        //DB �߰�
        string query = $"INSERT INTO character_info (Nickname, CharacterType) VALUES ('{Nickname}', '{characterType}')";
        bool isSuccess = OnInsertOnUpdateRequest(query);

        if (isSuccess)
        {
            //�迭 �߰�
            networkManager.OwnedCharacters.Add(characterType);
        }
        else
        {
            Debug.LogError("Failed to insert character info.");
        }
    }
    public void UpdatePlayerGold( int newGoldAmount)
    {
        string query = $"UPDATE u_info SET Money = {newGoldAmount} WHERE Nickname = '{Nickname}'";

        using (MySqlCommand sqlCommand = new MySqlCommand(query, _dbConnection))
        {
            _dbConnection.Open();
            sqlCommand.ExecuteNonQuery();
            _dbConnection.Close();
        }
    }
    public void UpdatePlayerCharacterId(string nameed)
    {
        string escapedName = MySqlHelper.EscapeString(nameed);

        string query = $"UPDATE u_info SET Characterid = '{escapedName}' WHERE Nickname = '{Nickname}'";

        using (MySqlCommand sqlCommand = new MySqlCommand(query, _dbConnection))
        {
            _dbConnection.Open();
            sqlCommand.ExecuteNonQuery();
            _dbConnection.Close();
        }
        CurrentPrafab = nameed;
        networkManager.ChangeChar(nameed);
        Debug.Log(query);
    }
}
