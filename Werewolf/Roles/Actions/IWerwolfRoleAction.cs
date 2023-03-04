using Werewolf.Game;

namespace Werewolf.Roles.Actions
{
    public interface IWerwolfRoleAction
    {
        string ID { get; }
        string Name { get; }

        string Description { get; }

        bool IsRequired { get; }

        bool IsActive { get; set; }

        bool IsUnique { get; }

        WerwolfPlayer Player { get; }

        IWerwolfRoleDescription Role { get; }

        void Deactivate();
        void Reactivate();

        bool CanPerform(WerwolfGame game, WerwolfPlayer onPlayer);

        void Perform(WerwolfGame game, WerwolfPlayer onPlayer);

        void BotPerform(WerwolfGame game);

        IWerwolfRoleAction GetAssigned(WerwolfPlayer player, IWerwolfRoleDescription role);

        void AfterRound(WerwolfGame game);

    }


}
