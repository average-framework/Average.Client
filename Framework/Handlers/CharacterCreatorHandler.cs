using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Client.Scripts;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class CharacterCreatorHandler : IHandler
    {
        private readonly UIService _uiService;
        private readonly CharacterCreatorScript _characterCreator;
        private readonly MenuService _menuService;

        public CharacterCreatorHandler(UIService uiService, CharacterCreatorScript characterCreator, MenuService menuService)
        {
            _uiService = uiService;
            _characterCreator = characterCreator;
            _menuService = menuService;
        }

        [ClientEvent("character-creator:start_creator")]
        private async void OnStartCreator()
        {
            await _uiService.ShutdownLoadingScreen();
            await _uiService.FadeIn();

            _characterCreator.StartCreator();
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
                    if (!IsCamInterpolating(_characterCreator.defaultCamera) && !IsCamInterpolating(_characterCreator.faceCamera) && !IsCamInterpolating(_characterCreator.bodyCamera) && !IsCamInterpolating(_characterCreator.footCamera))
                    {
                        if (_characterCreator.currentCamIndex < 3)
                        {
                            _characterCreator.currentCamIndex += 1;

                            _characterCreator.SwitchCamera(_characterCreator.currentCamIndex);

                            switch (_characterCreator.currentCamIndex)
                            {
                                case 0:
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(_characterCreator.faceCamera, _characterCreator.defaultCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(_characterCreator.bodyCamera, _characterCreator.faceCamera, 750, 1, 0);
                                    break;
                                case 3:
                                    SetCamActiveWithInterp(_characterCreator.footCamera, _characterCreator.bodyCamera, 750, 1, 0);
                                    break;
                            }
                        }
                    }
                }
                else if (key == 40)
                {
                    // Bottom
                    if (!IsCamInterpolating(_characterCreator.defaultCamera) && !IsCamInterpolating(_characterCreator.faceCamera) && !IsCamInterpolating(_characterCreator.bodyCamera) && !IsCamInterpolating(_characterCreator.footCamera))
                    {
                        if (_characterCreator.currentCamIndex > 0)
                        {
                            _characterCreator.currentCamIndex -= 1;

                            _characterCreator.SwitchCamera(_characterCreator.currentCamIndex);

                            switch (_characterCreator.currentCamIndex)
                            {
                                case 0:
                                    SetCamActiveWithInterp(_characterCreator.defaultCamera, _characterCreator.faceCamera, 750, 1, 0);
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(_characterCreator.faceCamera, _characterCreator.bodyCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(_characterCreator.bodyCamera, _characterCreator.footCamera, 750, 1, 0);
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
