using UnityEngine;
using System.Collections;

public class JsonType
{
    public string userName;
    public float betAmount;
    public int mineNum;
    public bool isBetting;
    public int posIndex;
    public bool canCashOut;
    public bool pressedCashOut;
    public string token;
    public float amount;
}

public class ReceiveJsonObject
{
    public double amount;
    public bool gameResult;
    public float earnAmount;
    public int randomNumber;
    public int[] randomArray;
    public int mineNum;
    public int gemNum;
    public float nextTileProfitAmount;
    public float totalProfitAmount;
    public float nextTileProfitCross;
    public float totalProfitCross;
    public bool isBetting;
    public int posIndex;
    public bool canCashOut;
    public string errMessage;
    public ReceiveJsonObject()
    {
    }
    public static ReceiveJsonObject CreateFromJSON(string data)
    {
        return JsonUtility.FromJson<ReceiveJsonObject>(data);
    }
}