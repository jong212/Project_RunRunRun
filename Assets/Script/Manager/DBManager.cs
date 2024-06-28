using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections.Generic;


public class DBManager : MonoBehaviour
{

    public NetworkManager networkManager;
    [Header("UI")]

    // 로그인 Ui
    [SerializeField] InputField Input_Id;
    [SerializeField] InputField Input_Pw;
    [SerializeField] Text Input_CheckIdPw_Error;
    [SerializeField] Text Text_DBResult;
    [SerializeField] Text Text_Log;

    // 회원가입 Ui
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
    public string Nickname { get => nickname; set => nickname = value; }
    
    private string _getId = "SELECT * FROM u_info where Nickname =";
    public string GetIdQuery { get => _getId; }
    private bool _idchk;
    public bool _IdChk { get => _idchk; set => _idchk = value; }

    private bool _isConnectTestComplete; //중요하진 않음
    private static MySqlConnection _dbConnection;
    private string SendQuery(string queryStr, string tableName)
    {
        //여기로 들어온 쿼리문에 SELECT가 포함되어 있으면 if 탐
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
            Text_Log.text = "성공";
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"e: {e.ToString()}");
            //Text_Log.text = "DB연결 실패";
            Debug.LogWarning("[1.디비 연결 실패]");
            return false;
        }
    }




    // 로그인 버튼  
    public void OnSubmit_Login()
    {
        string query = string.Empty;
        if (_isConnectTestComplete == false)
        {
            Text_Log.text = "DB 연결을 먼저 시도해주세요";
            return;
        }

        Text_Log.text = string.Empty;
        if (string.IsNullOrWhiteSpace(Input_Id.text) || string.IsNullOrWhiteSpace(Input_Pw.text))
        {
            Input_CheckIdPw_Error.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }
        else
        {
            query = $"SELECT Password FROM u_info WHERE Nickname = '{Input_Id.text}'";
        }

        string result = SendQuery(query, "u_info");

        if (string.IsNullOrEmpty(result))
        {
            Input_CheckIdPw_Error.text = "ID가 존재하지 않습니다.";
            return;
        }

        string retrievedPassword = ExtractPassword(result);

        if (retrievedPassword == Input_Pw.text)
        {

            Input_CheckIdPw_Error.text = "로그인 성공!";
            //여기에 네트워크 매니저 connect 함수를 호출하고 싶어
            Nickname = Input_Id.text;
            List<string> temp = GetMycharacter(Nickname);
            foreach (string characterType in temp)
            {
                networkManager.OwnedCharacters.Add(characterType);
            }
            CurrentPrafab = GetCharacterId(Nickname);
            networkManager.Connect(CurrentPrafab, Nickname);


        }
        else
        {
            Input_CheckIdPw_Error.text = "비밀번호가 일치하지 않습니다.";
        }
    }

    // 로그인 버튼 - 비밀번호 값 만 가져오기
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
            Text_Log.text = "DB 연결을 먼저 시도해주세요";
            return;
        }
        if (string.IsNullOrWhiteSpace(Input_JoinId.text))
        {
            Input_JoinIdMessage.text = "아이디를 입력해 주세요";
        }
        else
        {
            query = $"SELECT Password FROM u_info WHERE Nickname = '{Input_JoinId.text}'";
            string result = SendQuery(query, "u_info");

            if (string.IsNullOrEmpty(result))
            {
                Input_JoinIdMessage.text = "사용 가능";
                _IdChk = true;
                return;
            }
            else
            {
                Input_JoinIdMessage.text = "사용중인 아이디";
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
            Debug.Log("플레이어 아이디 없음");
        }
        return result;
    }
    //회원가입 완료 팝업
    public void OnSubmit_Join_success()
    {
        if (!_IdChk)
        {
            Input_JoinIdMessage2.text = "아이디 중복체크 필수";
        }
        else if (string.IsNullOrEmpty(Input_JoinPw.text))
        {
            Input_JoinIdMessage2.text = "비밀번호를 입력해주세요";
        }
        else if (Input_JoinPw.text != Input_JoinPwChk.text)
        {
            Input_JoinIdMessage2.text = "비밀번호를 서로 다르게 입력함";
        }
        else
        {
            string query = $"INSERT INTO u_info (Nickname, Password) VALUES ('{Input_JoinId.text}', '{Input_JoinPw.text}')";
            bool isSuccess = OnInsertOnUpdateRequest(query);

            Btn_confirm.SetActive(!Btn_confirm.activeSelf);
        }
    }

    // 플레이어 캐릭터 아이디 가져오기
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
    public void InsertCharacterInfo( string characterType)
    {
        //DB 추가
        string query = $"INSERT INTO character_info (Nickname, CharacterType) VALUES ('{Nickname}', '{characterType}')";
        bool isSuccess = OnInsertOnUpdateRequest(query);

        if (isSuccess)
        {
            //배열 추가
            networkManager.OwnedCharacters.Add(characterType);
        }
        else
        {
            Debug.LogError("Failed to insert character info.");
        }
    }
}
