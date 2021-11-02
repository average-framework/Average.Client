using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class CharacterCreatorHandler : IHandler
    {
        private readonly UIService _uiService;
        private readonly CharacterCreatorService _characterCreatorService;
        private readonly MenuService _menuService;

        public CharacterCreatorHandler(UIService uiService, CharacterCreatorService characterCreatorService, MenuService menuService)
        {
            _uiService = uiService;
            _characterCreatorService = characterCreatorService;
            _menuService = menuService;
        }

        [ClientEvent("character-creator:start_creator")]
        private async void OnStartCreator()
        {
            await _uiService.ShutdownLoadingScreen();
            await _uiService.FadeIn();

            _characterCreatorService.StartCreator();
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> args, CallbackDelegate result)
        {
            return result;
        }

        [UICallback("frame_ready")]
        private CallbackDelegate OnFrameReady(IDictionary<string, object> args, CallbackDelegate result)
        {
            _uiService.Show("menu");
            return result;
        }

        [UICallback("menu/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> args, CallbackDelegate result)
        {
            if (_menuService.IsOpen)
            {
                var key = int.Parse(args["key"].ToString());

                if (key == 37)
                {
                    // Left
                    var heading = GetEntityHeading(PlayerPedId());
                    heading -= 45f;

                    TaskAchieveHeading(PlayerPedId(), heading, 750);
                }
                else if (key == 39)
                {
                    // Right
                    var heading = GetEntityHeading(PlayerPedId());
                    heading += 45f;

                    TaskAchieveHeading(PlayerPedId(), heading, 750);
                }
                else if (key == 38)
                {
                    // Top
                    if (!IsCamInterpolating(_characterCreatorService.defaultCamera) && !IsCamInterpolating(_characterCreatorService.faceCamera) && !IsCamInterpolating(_characterCreatorService.bodyCamera) && !IsCamInterpolating(_characterCreatorService.footCamera))
                    {
                        if (_characterCreatorService.currentCamIndex < 3)
                        {
                            _characterCreatorService.currentCamIndex += 1;

                            _characterCreatorService.SwitchCamera(_characterCreatorService.currentCamIndex);

                            switch (_characterCreatorService.currentCamIndex)
                            {
                                case 0:
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(_characterCreatorService.faceCamera, _characterCreatorService.defaultCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(_characterCreatorService.bodyCamera, _characterCreatorService.faceCamera, 750, 1, 0);
                                    break;
                                case 3:
                                    SetCamActiveWithInterp(_characterCreatorService.footCamera, _characterCreatorService.bodyCamera, 750, 1, 0);
                                    break;
                            }
                        }
                    }
                }
                else if (key == 40)
                {
                    // Bottom
                    if (!IsCamInterpolating(_characterCreatorService.defaultCamera) && !IsCamInterpolating(_characterCreatorService.faceCamera) && !IsCamInterpolating(_characterCreatorService.bodyCamera) && !IsCamInterpolating(_characterCreatorService.footCamera))
                    {
                        if (_characterCreatorService.currentCamIndex > 0)
                        {
                            _characterCreatorService.currentCamIndex -= 1;

                            _characterCreatorService.SwitchCamera(_characterCreatorService.currentCamIndex);

                            switch (_characterCreatorService.currentCamIndex)
                            {
                                case 0:
                                    SetCamActiveWithInterp(_characterCreatorService.defaultCamera, _characterCreatorService.faceCamera, 750, 1, 0);
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(_characterCreatorService.faceCamera, _characterCreatorService.bodyCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(_characterCreatorService.bodyCamera, _characterCreatorService.footCamera, 750, 1, 0);
                                    break;
                                case 3:
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
