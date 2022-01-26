using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using SimpleJSON;
public class UIManager : MonoBehaviour
{
    public GameObject betPanel;
    public GameObject cashOutPanel;
    public GameObject CashOutBtn;
    public GameObject gameResultPanel;

    public TMP_Text info_Text;
    public TMP_Text resultText;
    public TMP_Text walletAmount_Text;

    public TMP_InputField AmountField;
    public TMP_InputField BombNumField;

    public TMP_InputField CashBombNumField;
    public TMP_InputField CashGemNumField;
    public TMP_InputField NextTileProfitField;
    public TMP_InputField TotalProfitField;

    public TMP_Text NextProfitText;
    public TMP_Text TotalProfitText;
    public TMP_Text BenefitText;
    public TMP_InputField BenefitField;

    BetPlayer _player;

    public GameObject[] Cards = new GameObject[25];

    public Texture MineImage;
    public Texture DiamondImage;
    public Texture BackCardImage;

    private int stateFlag = 0;
    private float betNumber;
    public SocketIOController io;

    private bool isbetting = false;
    private float betTime = 0;
    private int bombNum = 2;

    // GameReadyStatus Send
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);

    // Start is called before the first frame update
    void Start()
    {
        betPanel.SetActive(true);
        cashOutPanel.SetActive(false);
        gameResultPanel.SetActive(false);

        _player = new BetPlayer();

        bombNum = 2;
        AmountField.text = "10.0";
        BombNumField.text = bombNum.ToString();

        io.Connect();

        io.On("connect", (e) =>
        {
            Debug.Log("Game started");
            io.On("mine position", (res) =>
            {
                PositionInfo(res);
            });

            io.On("card result", (res) =>
            {
                CardResult(res);
            });

            io.On("game result", (res) =>
            {
                GameResult(res);
            });

            io.On("error message", (res) =>
            {
                ShowError(res);
            });
        });        

        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowError(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        switch (res.errMessage)
        {
            case "0":
                info_Text.text = "Bet Error!";
                break;
            case "1":
                info_Text.text = "Can't find Server!";
                betPanel.SetActive(true);
                cashOutPanel.SetActive(false);
                break;
        }
        //info_Text.text = res.errMessage.ToString();
    }
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        Debug.Log("token=--------" + usersInfo["token"]);
        Debug.Log("amount=------------" + usersInfo["amount"]);
        Debug.Log("userName=------------" + usersInfo["userName"]);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];

        float i_balance = float.Parse(usersInfo["amount"]);
        walletAmount_Text.text = i_balance.ToString("F3");
    }

    void PositionInfo(SocketIOEvent socketIOEvent)
    {
        betPanel.SetActive(false);
        cashOutPanel.SetActive(true);
        gameResultPanel.SetActive(false);
        SetInitImage();

        isbetting = true;

        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        walletAmount_Text.text = res.amount.ToString("F3");
        CashBombNumField.text = res.mineNum.ToString();
        CashGemNumField.text = res.gemNum.ToString();
        NextProfitText.text = "Profit On Next Tile (" + res.nextTileProfitCross.ToString("F2") + "X" + ")";
        TotalProfitText.text = "Total Profit (" + res.totalProfitCross.ToString("F2") + "X" + ")";
        NextTileProfitField.text = res.nextTileProfitAmount.ToString("F2");
        TotalProfitField.text = res.totalProfitAmount.ToString("F2");

        if (res.canCashOut)
            CashOutBtn.SetActive(true);
        else
            CashOutBtn.SetActive(false);
    }

    void CardResult(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        if (res.isBetting)
        {
            if (res.canCashOut)
                CashOutBtn.SetActive(true);
            SetImageOfIndex(res.posIndex, true);

            NextProfitText.text = "Profit On Next Tile (" + res.nextTileProfitCross.ToString("F2") + "X" + ")";
            TotalProfitText.text = "Total Profit (" + res.totalProfitCross.ToString("F2") + "X" + ")";
            NextTileProfitField.text = res.nextTileProfitAmount.ToString("F2");
            TotalProfitField.text = res.totalProfitAmount.ToString("F2");
        }
        else if (!res.isBetting)
        {
            gameResultPanel.SetActive(true);
            resultText.text = "You Lose!";
            BenefitField.text = "0.00";
            BenefitText.text = "0.0X";
            cashOutPanel.SetActive(false);
            betPanel.SetActive(true);
            SetImage(res.randomArray);
        }

    }

    void GameResult(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        walletAmount_Text.text = res.amount.ToString("F3");

        gameResultPanel.SetActive(true);
        betPanel.SetActive(true);
        cashOutPanel.SetActive(false);
        resultText.text = "You Earned " + AmountField.text + " + " + (res.totalProfitAmount - float.Parse(AmountField.text)).ToString("F2") +"!";
        BenefitField.text = res.totalProfitAmount.ToString("F2");
        BenefitText.text = res.totalProfitCross.ToString("F2")+"X";
        SetImage(res.randomArray);

    }

    public void MinusBtn_Clicked()
    {
        if (bombNum > 2)
        {
            bombNum--;
            BombNumField.text = bombNum.ToString();
        }
    }

    public void PlusBtnClicked()
    {
        if (bombNum < 24)
        {
            bombNum++;
            BombNumField.text = bombNum.ToString();
        }
    }

    public void MinBtn_Clicked()
    {
        AmountField.text = "10.0";
    }

    public void CrossBtn_Clicked()
    {
        float amount = float.Parse(AmountField.text);
        if (amount >= 100000f)
            AmountField.text = "100000.0";
        else
            AmountField.text = (amount * 2.0f).ToString("F2");
    }

    public void HalfBtn_Clicked()
    {
        float amount = float.Parse(AmountField.text);
        if (amount <= 10f)
            AmountField.text = "10.0";
        else
            AmountField.text = (amount / 2.0f).ToString("F2");
    }

    public void MaxBtn_Clicked()
    {
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(walletAmount_Text.text) ? "0" : walletAmount_Text.text);
        if (myTotalAmount >= 100000f)
            AmountField.text = "100000.0";
        else if (myTotalAmount >= 10f && myTotalAmount < 100000f)
            AmountField.text = myTotalAmount.ToString("F2");
    }

    public void AmountField_Changed()
    {
        if (float.Parse(AmountField.text) <= 10f)
            AmountField.text = "10.0";
        else if (float.Parse(AmountField.text) >= 100000f)
        {
            AmountField.text = "100000.0";
        }        
    }

    public void BetBtnClicked()
    {
        info_Text.text = "";
        JsonType JObject = new JsonType();
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(walletAmount_Text.text) ? "0" : walletAmount_Text.text);
        float betamount = float.Parse(string.IsNullOrEmpty(AmountField.text) ? "0" : AmountField.text);
        if (betamount <= myTotalAmount)
        {
            JObject.userName = _player.username;
            JObject.betAmount = betamount;
            JObject.mineNum = int.Parse(BombNumField.text);
            JObject.isBetting = true;
            JObject.token = _player.token;
            JObject.amount = myTotalAmount;
            io.Emit("bet info", JsonUtility.ToJson(JObject));                       
        }
        else
            info_Text.text = "Not enough Funds";
    }

    public void ClickCardEvent(int posIndex)
    {
        JsonType obj = new JsonType();
        obj.posIndex = posIndex;
        obj.token = _player.token;
        if (isbetting)
        {
            try
            {
                io.Emit("card click", JsonUtility.ToJson(obj));
            }
            catch
            {
                info_Text.text = "Can't Connect with server!";
            }
        }           
    }

    public void CashOutBtnClicked()
    {
        JsonType obj = new JsonType();
        obj.pressedCashOut = true;
        obj.token = _player.token;
        try
        {
            io.Emit("cash out", JsonUtility.ToJson(obj));
        }
        catch
        {
            info_Text.text = "Can't Connect with server!";
        }
    }

    void SetImage(int[] imageArray)
    {
        for(int i = 0; i < 25; i++)
        {
            if (imageArray[i] == 0)
                Cards[i].GetComponent<RawImage>().texture = DiamondImage;
            else if(imageArray[i] == 1)
                Cards[i].GetComponent<RawImage>().texture = MineImage;
        }
    }

    void SetInitImage()
    {
        for (int i = 0; i < 25; i++)
        {            
            Cards[i].GetComponent<RawImage>().texture = BackCardImage;           
        }
    }    

    void SetImageOfIndex( int posIndex, bool isBetting)
    {
        if(isBetting)
            Cards[posIndex].GetComponent<RawImage>().texture = DiamondImage;
        else
            Cards[posIndex].GetComponent<RawImage>().texture = MineImage;
    }

    public class BetPlayer
    {
        public string username;
        public string token;
    }
}
