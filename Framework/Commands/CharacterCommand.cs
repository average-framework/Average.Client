using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;

namespace Average.Client.Framework.Commands
{
    internal class CharacterCommand : ICommand
    {
        private readonly CharacterService _characterService;

        public CharacterCommand(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [ClientCommand("character.set_ped")]
        private void OnSetPed(string hashName, int variante)
        {
            _characterService.SetTemporaryPed(hashName, variante);
        }

        [ClientCommand("character.set_damage_pack")]
        private void OnSetDamagePack(string pack, float damage, float multiplier)
        {
            _characterService.SetDamagePack(pack, damage, multiplier);
        }

        [ClientCommand("character.clear_damage_pack")]
        private void OnClearDamagePack()
        {
            _characterService.ClearDamagePack();
        }
    }
}
