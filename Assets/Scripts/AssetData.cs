using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetData
{
    public enum CURRENCY
    {
        Default, EmojiCoins
    }
    private string _id;
    private string _name;
    private string _thumbnailUrl;
    private float _price;
    private CURRENCY _currency;
    
    public AssetData(string id, string name, string thumbnailUrl,
        float price, CURRENCY currency)
    {
        ID = id;
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Price = price;
        Currency = currency;
    }
    
    public string ID
    {
        get => _id;
        set => _id = value;
    }
    
    public string Name
    {
        get => _name;
        set => _name = value;
    }
    
    public string ThumbnailUrl
    {
        get => _thumbnailUrl;
        set => _thumbnailUrl = value;
    }
    
    public float Price
    {
        get => _price;
        set => _price = value;
    }

    public CURRENCY Currency
    {
        get => _currency;
        set => _currency = value;
    }
    
    public override string ToString()
    {
        return ($"Asset with id:{ID} name:{Name} thumbnail url:{ThumbnailUrl} " +
                $"price:{Price} currency:{Currency.ToString()}");
    }
}
