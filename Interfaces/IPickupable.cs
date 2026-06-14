using TheAdventure.Models;

namespace TheAdventure.Interfaces;

public interface IPickupable
{
    void Apply(PlayerObject player);
    bool IsInRange(int playerX, int playerY);
}
