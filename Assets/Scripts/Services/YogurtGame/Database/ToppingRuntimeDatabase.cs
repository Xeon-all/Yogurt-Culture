using System.Collections.Generic;
using Excel2Unity;
using UnityEngine;

public class ToppingRuntimeDatabase : Database<ToppingData>
{
    public override string TableShortName => "Topping";
    public ToppingRuntimeDatabase(TableDataBase[] rows) 
        : base((ToppingData[])rows) 
    {
        foreach(var item in rows)
            _inventory.Add(
                item.ID, 
                new ToppingItem(item as ToppingData)
            );
    }
}
