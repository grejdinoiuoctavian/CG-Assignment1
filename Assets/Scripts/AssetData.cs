using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetData
{
    private string _id;
    private string _name;
    private string _thumbnailUrl;
    private int _price;
    private bool _owned;
    
    public AssetData(string id, string name, string thumbnailUrl,
        int price, bool owned)
    {
        ID = id;
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Price = price;
        Owned = owned;
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
    
    public int Price
    {
        get => _price;
        set => _price = value;
    }

    public bool Owned
    {
        get => _owned;
        set => _owned = value;
    }
    
    public override string ToString()
    {
        return ($"Asset with id:{ID} name:{Name} thumbnail url:{ThumbnailUrl} " +
                $"price:{Price} owned:{Owned}");
    }
}
