using System.Collections.Generic;

public interface IInventoryStorage
{
    List<UserItem> LoadUserItems();
    void SaveUserItems(List<UserItem> items);
}
