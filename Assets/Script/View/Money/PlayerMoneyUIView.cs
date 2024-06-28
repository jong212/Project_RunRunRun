using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoneyUIView : MonoBehaviour
{
    public NetworkManager networkManager;
    [SerializeField] Text _currentGoldText;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if(networkManager != null)
        {
            if(_currentGoldText != null)
            {
                _currentGoldText.text = networkManager.currentMoney.ToString();
            } 
            
        }
        
    }
}
