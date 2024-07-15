using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    [SerializeField] InputField Input_Id;
    [SerializeField] InputField Input_Pw;

    public string GetInputId() { return Input_Id.text; }
    public string GetInputPw() { return Input_Pw.text; }

    public void ResetInputField()
    {
        Input_Id.text = string.Empty;
        Input_Pw.text = string.Empty;
    }
}
