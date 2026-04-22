using System.Collections.Generic;
using Excel2Unity;

public class YogurtRuntimeDatabase : Database<YogurtDatabase>
{
    public override string TableShortName => "Yogurt";
    public YogurtRuntimeDatabase(TableDataBase[] rows) : base((YogurtDatabase[])rows){}
}
