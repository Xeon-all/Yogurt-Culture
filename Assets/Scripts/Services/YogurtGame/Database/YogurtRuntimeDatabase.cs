using System.Collections.Generic;
using Excel2Unity;

public class YogurtRuntimeDatabase : Database<YogurtData>
{
    public override string TableShortName => "Yogurt";
    public YogurtRuntimeDatabase(TableDataBase[] rows) 
        : base()
    {
        foreach(var item in rows)
            _inventory.Add(
                item.ID, 
                new YogurtItem(item as YogurtData, 10)
            );
    }
}
