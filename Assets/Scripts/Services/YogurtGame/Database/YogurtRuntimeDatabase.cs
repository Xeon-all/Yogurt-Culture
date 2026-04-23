using System.Collections.Generic;
using Excel2Unity;

public class YogurtRuntimeDatabase : Database<YogurtDatabase>
{
    public override string TableShortName => "Yogurt";
    public YogurtRuntimeDatabase(TableDataBase[] rows) 
        : base((YogurtDatabase[])rows)
    {
        foreach(var item in rows)
            _inventory.Add(
                item.ID, 
                new YogurtItem(item as YogurtDatabase)
            );
    }
}
