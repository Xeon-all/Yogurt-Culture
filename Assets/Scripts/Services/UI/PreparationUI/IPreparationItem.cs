using Excel2Unity;

public interface IPreparationItem<T> where T : TableDataBase
{
    void SetUpItem(Itembase<T> item);
    void UpdateDisplay();
}