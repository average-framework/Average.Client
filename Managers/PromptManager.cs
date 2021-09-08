using System.Collections.Generic;
using System.Threading.Tasks;
using SDK.Client;
using SDK.Client.Interfaces;
using SDK.Client.Prompts;
using SDK.Shared;
using SDK.Shared.Threading;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class PromptManager : InternalPlugin, IPromptManager
    {
        private readonly List<IPromptInput> _prompts = new();

        [Thread]
        private async Task Update()
        {
            for (int i = 0; i < _prompts.Count; i++)
            {
                var prompt = _prompts[i];
                var predicate = prompt.Condition();
                var visibilityPredicate = prompt.VisibilityCondition();
                var enabledPredicate = prompt.EnabledCondition();

                UIPromptSetVisible(prompt.Id, prompt.IsVisible && visibilityPredicate);
                UIPromptSetEnabled(prompt.Id, prompt.IsEnabled && enabledPredicate);
                
                if (predicate)
                {
                    switch (prompt)
                    {
                        case StandardPrompt p:
                            if (Call<bool>(0x2787CC611D3FACC5, p.Id) && !p.IsRunningCompleted)
                            {
                                p.IsRunningCompleted = true;
                                p.OnStandardModeCompletedReached(p);
                            }
                            break;
                        case HoldPrompt p:
                            if (Call<bool>(0xC7D70EAEF92EFF48, p.Id) && !p.IsRunningCompleted)
                            {
                                p.IsRunningCompleted = true;
                                p.OnHoldModeRunningReached(p);
                            }

                            if (Call<bool>(0xE0F65F0640EF0617, p.Id) && !p.IsCompletedReached)
                            {
                                p.IsCompletedReached = true;
                                p.OnHoldModeCompletedReached(p);
                            }
                            break;
                    }
                }
            }
        }

        public void Create(IPromptInput prompt) => _prompts.Add(prompt);

        public bool ExistById(int id) => _prompts.Exists(x => x.Id == id);
        public IPromptInput GetById(int id) => _prompts.Find(x => x.Id == id);

        public void Delete(IPromptInput prompt)
        {
            // remove group
            Call(0x4E52C800A28F7BE8, prompt.Id, 0);
            // remove prompt
            Call(0x00EDE88D4D13CF59, prompt.Id);
            
            _prompts.Remove(prompt);
        }
        
        public void ClearAll()
        {
            for (int i = 0; i < _prompts.Count; i++)
            {
                var prompt = _prompts[i];
                Call(0x4E52C800A28F7BE8, prompt.Id, 0);
                // remove prompt
                Call(0x00EDE88D4D13CF59, prompt.Id);
            }
        }

        #region Event

        [ClientEvent("ResourceStop")]
        private void OnResourceStop(string resource)
        {
            if (resource == Constant.RESOURCE_NAME)
            {
                ClearAll();
                Dispose();
            }
        }

        #endregion
    }
}