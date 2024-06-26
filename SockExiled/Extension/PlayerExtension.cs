using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Items;

namespace SockExiled.Extension
{
    internal static class PlayerExtension
    {
        public static string ToRappresentativeString(this Player player)
        {
            if (player is null)
                return "pl_null";

            string Inventory = string.Empty;
            string Effects = string.Empty;

            if (player.IsAlive)
            {
                foreach (Item Item in player.Items)
                {
                    Inventory += Item.Serial;
                }

                foreach (StatusEffectBase Effect in player.ActiveEffects)
                {
                    Effects += Effect.name;
                }
            }

            //  {player.DisplayNickname ?? "nothin'"} {player.Id} {player.UserId} {player.CurrentArmor?.Serial.ToString() ?? "-1"} {player.CurrentItem?.Serial.ToString() ?? "-1"} {player.CurrentRoom?.Identifier.name ?? "-2"} {player.IsAlive} {player.Role?.Type.ToString() ?? "Unknown"} {player.Scale.ToString() ?? "0,0,0"} {player.IsCuffed} {Inventory} {Effects}
            return $"{player.Nickname} {player.DisplayNickname ?? "nothin'"} {player.Id} {player.UserId} {player.CurrentArmor?.Serial.ToString() ?? "-1"} {player.CurrentItem?.Serial.ToString() ?? "-1"} {player.CurrentRoom?.Identifier.name ?? "-2"} {player.IsAlive} {player.Role?.Type.ToString() ?? "Unknown"} {player.Scale.ToString() ?? "0,0,0"} {player.IsCuffed} {Inventory} {Effects}";
        }
    }
}
