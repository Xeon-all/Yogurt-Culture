using System.Collections.Generic;
using Excel2Unity;

public class ToppingRuntimeDatabase : Database<ToppingData>
{
    public override string TableShortName => "Topping";
    public ToppingRuntimeDatabase(TableDataBase[] rows) : base((ToppingData[])rows) {}
}
