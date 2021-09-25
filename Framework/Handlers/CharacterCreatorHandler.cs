using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;

namespace Average.Client.Framework.Handlers
{
    internal class CharacterCreatorHandler : IHandler
    {
        private readonly UIService _uiService;
        private readonly CharacterCreatorService _characterCreatorService;

        public CharacterCreatorHandler(UIService uiService, CharacterCreatorService characterCreatorService)
        {
            _uiService = uiService;
            _characterCreatorService = characterCreatorService;
        }

        [ClientEvent("character-creator:start_creator")]
        private async void OnStartCreator()
        {
            await _uiService.ShutdownLoadingScreen();
            await _uiService.FadeIn();
            _characterCreatorService.StartCreator();
        }
    }
}
