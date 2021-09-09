using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using SDK.Client.Interfaces;
using SDK.Client.Menu;
using SDK.Client.Models;
using SDK.Client.Prompts;
using SDK.Client.Utils;
using SDK.Shared.DataModels;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class EnterpriseManager : InternalPlugin,  IEnterpriseManager
    {
        private MenuContainer _mainMenu;
        private MenuContainer _employeesManagementMenu;
        private MenuContainer _employeeMenu;
        private MenuContainer _recruitMenu;
        private MenuContainer _recruitEmployeeMenu;
        private MenuContainer _recruitPromoteMenu;
        private MenuContainer _treasuryMenu;

        private EnterpriseModel _currentEnterprise;

        private bool _lastIsNear;
        private bool _isNear;

        private Dictionary<string, EnterpriseModel> _enterprises { get; } = new();
        private List<CharacterData> _charactersInfos = new();

        private EnterpriseData _currentEnterpriseData;
        private List<CharacterData> _currentEnterpriseEmployeesData = new();

        private HoldPrompt _promptPrompt;
        
        public override void OnInitialized()
        {
            Task.Factory.StartNew(async () =>
            {
                await Character.IsReady();
                Thread.StartThread(KeyboardUpdate);
            });
        }

        public void RegisterJob(string jobName, EnterpriseModel enterprise)
        {
            if (!_enterprises.ContainsKey(jobName))
            {
                _enterprises.Add(jobName, enterprise);
            }
        }

        public EnterpriseModel GetEnterpriseModel(string jobName)
        {
            if (_enterprises.ContainsKey(jobName))
                return _enterprises[jobName];

            return null;
        }
        
        public bool EnterpriseExist(string jobName) => _enterprises.ContainsKey(jobName);

        #region Task

        private async Task KeyboardUpdate()
        {
            if (Character.Current == null) return;

            var ped = PlayerPedId();
            var pos = GetEntityCoords(ped, true, true);

            if (!_enterprises.ContainsKey(Character.Current.Job.Name)) return;

            var enterprise = _enterprises[Character.Current.Job.Name];
            if (enterprise == null) return;

            var nearest = GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, enterprise.Interact.Position.X,
                enterprise.Interact.Position.Y, enterprise.Interact.Position.Z, true) <= enterprise.Interact.Radius;

            if (!nearest)
            {
                if (_isNear)
                {
                    _isNear = false;
                    
                    Prompt.Delete(_promptPrompt);
                }

                var showCircle =
                    GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, enterprise.Interact.Position.X,
                        enterprise.Interact.Position.Y, enterprise.Interact.Position.Z, true) <= enterprise.Interact.Radius + 15f;

                if (showCircle)
                {
                    Call(0x2A32FAA57B937173, (uint) MarkerType.Halo, enterprise.Interact.Position.X,
                        enterprise.Interact.Position.Y, enterprise.Interact.Position.Z - 0.98f, 0, 0, 0, 0, 0, 0,
                        enterprise.Interact.Radius, enterprise.Interact.Radius, 0.2f, 255, 255, 255, 255, 0, 0, 2,
                        0, 0, 0, 0);
                }
                else
                {
                    await BaseScript.Delay(250);
                }
            }
            else
            {
                if (!_isNear)
                {
                    _isNear = true;
                    
                    _promptPrompt = new HoldPrompt(0, "Gérer l'entreprise", 1000, (uint) Control.Revive, () =>
                    {
                        return Character.Current.Job.Name == enterprise.JobName &&
                               enterprise.Roles.Exists(x => x.Name == Character.Current.Job.Role.Name);
                    }, () =>
                    {
                        return GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, enterprise.Interact.Position.X, enterprise.Interact.Position.Y, enterprise.Interact.Position.Z, true) <= enterprise.Interact.Radius + 1f;
                    }, () =>
                    {
                        return Character.Current.Job.Name == enterprise.JobName &&
                               enterprise.Roles.Exists(x => x.Name == Character.Current.Job.Role.Name) && GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, enterprise.Interact.Position.X, enterprise.Interact.Position.Y, enterprise.Interact.Position.Z, true) <= enterprise.Interact.Radius;
                    });
                    _promptPrompt.OnHoldModeCompletedHandler += async prompt =>
                    {
                        _currentEnterprise = enterprise;
                        
                        await LoadEnterprise(_currentEnterprise.JobName);
                        await LoadEmployees(_currentEnterprise.JobName);
                        
                        Menu.CanCloseMenu = true;
                        InitMainMenu();
                        await Menu.OpenMenu(_mainMenu);
                        Focus();
                    };
                    Prompt.Create(_promptPrompt);
                }
            }
        }

        #endregion

        private async Task LoadCharacters()
        {
            _charactersInfos = null;

            Rpc.Event("Enterprise.GetAllCharactersInfos").On<List<CharacterData>>(data =>
            {
                _charactersInfos = data;
            }).Emit();

            while (_charactersInfos == null) await BaseScript.Delay(0);
        }

        private async Task<EnterpriseData> LoadEnterprise(string jobName)
        {
            Rpc.Event("Enterprise.Load").On<EnterpriseData>(data =>
            {
                _currentEnterpriseData = data;
            }).Emit(jobName);

            while (_currentEnterpriseData == null) await BaseScript.Delay(0);
            return _currentEnterpriseData;
        }

        private async Task LoadEmployees(string jobName)
        {
            _currentEnterpriseEmployeesData = null;

            Rpc.Event("Enterprise.LoadEmployees").On<List<CharacterData>>(data =>
            {
                _currentEnterpriseEmployeesData = data;
            }).Emit(jobName);

            while (_currentEnterpriseEmployeesData == null) await BaseScript.Delay(0);
        }

        private void InitMainMenu()
        {
            _mainMenu = new MenuContainer("ENTREPRISE", _currentEnterprise.JobLabel.ToUpper());

            InitEmployeesManagementMenu();
            InitRecruitMenu();
            InitTreasuryManagementMenu();

            _mainMenu.AddItem(new MenuButtonContainer("Gestion des employés", _employeesManagementMenu, RequirePermissionForMenu("can_manage_lower_employees")));
            _mainMenu.AddItem(new MenuButtonContainer("Recruter", _recruitMenu, RequirePermissionForMenu("can_manage_lower_employees")));
            _mainMenu.AddItem(new MenuButtonContainer("Trésorie", _treasuryMenu, RequirePermissionForMenu("can_manage_treasury")));
            _mainMenu.AddItem(new MenuButtonItem("Coffre", async item =>
            {
                Menu.CloseMenu();
                await Storage.Show("player_" + Character.Current.RockstarId);
                await Storage.Show(_currentEnterprise.ChestId);
                Storage.Open();
            }, RequirePermissionForMenu("can_manage_chest")));
        }

        private async void InitTreasuryManagementMenu()
        {
            _treasuryMenu = new MenuContainer(_currentEnterprise.JobLabel.ToUpper(),"TRESORIE");

            var enterprise = await LoadEnterprise(_currentEnterprise.JobName);
            var amountValue = new object();
            var entAmount = decimal.Parse(enterprise.TreasuryAmount);
            var treasuryItem = new MenuLabelItem($"Solde Trésorie {ConvertDecimalToString(entAmount)}");

            _treasuryMenu.AddItem(treasuryItem);
            _treasuryMenu.AddItem(new MenuTextboxItem("Montant", "", "$0", "", 1, 10, item => { amountValue = item; }));

            var canDeposit = true;
            var canWithdraw = true;

            _treasuryMenu.AddItem(new MenuButtonItem("Déposé", async item =>
            {
                if (canDeposit)
                {
                    canDeposit = false;

                    enterprise = await LoadEnterprise(_currentEnterprise.JobName);

                    var result = decimal.TryParse(amountValue.ToString(), out var value);
                    entAmount = decimal.Parse(enterprise.TreasuryAmount);

                    if (result)
                    {
                        if (value > 0 && value <= Character.Current.Economy.Money)
                        {
                            Character.RemoveMoney(value);
                            entAmount += value;
                            treasuryItem.Text = $"Solde Trésorie: {ConvertDecimalToString(entAmount)}";
                            Event.EmitServer("Enterprise.AddTreasury", enterprise.JobName, ConvertDecimalToString(entAmount));
                            await BaseScript.Delay(3000);
                            await LoadEnterprise(enterprise.JobName);

                            Notification.Schedule("TRAVAIL", $"Vous avez déposé ${value}.", 5000);
                        }
                        else
                        {
                            Notification.Schedule("TRAVAIL", "Montant trop élevé.", 5000);
                        }

                        canDeposit = true;
                    }
                    else
                    {
                        Notification.Schedule("TRAVAIL", "Montant invalide.", 5000);
                    }
                }
                else
                {
                    Notification.Schedule("TRAVAIL", "Anti spam.", 5000);
                }
            }));

            _treasuryMenu.AddItem(new MenuButtonItem("Retiré", async item =>
            {
                if (canWithdraw)
                {
                    canWithdraw = false;

                    enterprise = await LoadEnterprise(_currentEnterprise.JobName);

                    var result = decimal.TryParse(amountValue.ToString(), out var value);
                    entAmount = decimal.Parse(enterprise.TreasuryAmount);

                    if (result)
                    {
                        if (value > 0 && value <= entAmount)
                        {
                            Character.AddMoney(value);
                            entAmount -= value;
                            enterprise.TreasuryAmount = ConvertDecimalToString(entAmount);
                            treasuryItem.Text = $"Solde Trésorie: {ConvertDecimalToString(entAmount)}";
                            Event.EmitServer("Enterprise.RemoveTreasury", enterprise.JobName, ConvertDecimalToString(entAmount));
                            await BaseScript.Delay(1000);
                            await LoadEnterprise(enterprise.JobName);

                            Notification.Schedule("TRAVAIL", $"Vous avez retiré ${value}.", 5000);
                        }
                        else
                        {
                            Notification.Schedule("TRAVAIL", "Montant trop élevé.", 5000);
                        }

                        canWithdraw = true;
                    }
                    else
                    {
                        Notification.Schedule("TRAVAIL", "Montant invalide.", 5000);
                    }
                }
                else
                {
                    Notification.Schedule("TRAVAIL", "Anti spam.", 5000);
                }
            }));
        }

        private async void InitEmployeesManagementMenu()
        {
            _employeesManagementMenu = new MenuContainer(_currentEnterprise.JobLabel.ToUpper(), "GESTION DES EMPLOYES");

            await LoadCharacters();

            foreach (var c in _currentEnterpriseEmployeesData)
            {
                _employeesManagementMenu.AddItem(new MenuButtonItem($"{c.Firstname} {c.Lastname}", async item =>
                {
                    _employeeMenu = new MenuContainer(_currentEnterprise.JobLabel.ToUpper(), c.Firstname.ToUpper() + " " + c.Lastname.ToUpper());

                    _employeeMenu.AddItem(new MenuButtonItem($"Grade: {_currentEnterprise.Roles.Find(x => x.Name == c.Job.Role.Name).Label}"));

                    var haveGoodJob = _currentEnterprise.JobName == c.Job.Name;

                    _employeeMenu.AddItem(new MenuButtonItem("Promotion", async item =>
                    {
                        var jobRoles = Job.GetRolesByJob(c.Job.Name);
                        var targetRole = jobRoles.ToList().Find(x => x.Name == c.Job.Role.Name);
                        var myRole = jobRoles.ToList().Find(x => x.Name == Character.Current.Job.Role.Name);

                        if (targetRole != null)
                        {
                            _recruitPromoteMenu = new MenuContainer("ENTERPRISE", "PROMOTION");

                            foreach (var role in jobRoles.Reverse())
                            {
                                _recruitPromoteMenu.AddItem(new MenuButtonItem(_currentEnterprise.Roles.Find(x => x.Name == role.Name).Label, async item =>
                                {
                                    // Le joueur ne doit pas pouvoir se rétrogradé lui même
                                    if (c.RockstarId != Character.Current.RockstarId)
                                    {
                                        if (role.Level < myRole.Level) // < Peu uniquement promouvoir un grade inférieur au siens, <= Peu promouvoir un grade inférieur ou égale au siens
                                        {
                                            foreach (var it in _recruitPromoteMenu.Items)
                                                it.Visible = false;

                                            _recruitPromoteMenu.AddItem(new MenuButtonItem("Veuillez patientez.."));
                                            Menu.UpdateRender(_recruitPromoteMenu, null);
                                            var roleLevel = Job.GetRolesByJob(_currentEnterprise.JobName).ToList().Find(x => x.Name == role.Name).Level;
                                            Job.PromotePlayerByRockstarId(c.RockstarId, _currentEnterprise.JobName, role.Name, roleLevel);

                                            Notification.Schedule("TRAVAIL", $"Vous avez promu {{c.Firstname}} {c.Lastname} au grade de {_currentEnterprise.Roles.Find(x => x.Name == role.Name).Label}.", 5000);

                                            await BaseScript.Delay(1000);
                                            InitEmployeesManagementMenu();
                                            await BaseScript.Delay(1000);
                                            await Menu.OpenMenu(_mainMenu);
                                        }
                                        else
                                        {
                                            Notification.Schedule("TRAVAIL", "Impossible de définir le grade d'un supérieur.", 5000);
                                        }
                                    }
                                    else
                                    {
                                        Notification.Schedule("TRAVAIL", "Vous ne pouvez pas vous donnez de promotion !", 5000);
                                    }
                                }));
                            }

                            await Menu.OpenMenu(_recruitPromoteMenu);
                        }
                    }, haveGoodJob));

                    _employeeMenu.AddItem(new MenuButtonItem("Virer", async item =>
                    {
                        var jobRoles = Job.GetRolesByJob(c.Job.Name);
                        var role = jobRoles.ToList().Find(x => x.Name == c.Job.Role.Name);

                        if (role != null)
                        {
                            item.Visible = false;
                            Menu.UpdateRender(_employeeMenu, null);
                            Job.FiredPlayerByRockstarId(c.RockstarId, "unemployed", "", 0);

                            Notification.Schedule("TRAVAIL", $"Vous avez virer {c.Firstname} {c.Lastname}.", 5000);

                            await BaseScript.Delay(1000);
                            Menu.CloseMenu();
                            Unfocus();
                        }
                    }, haveGoodJob));

                    await Menu.OpenMenu(_employeeMenu);
                }));
            }
        }

        private async void InitRecruitMenu()
        {
            _recruitMenu = new MenuContainer(_currentEnterprise.JobLabel.ToUpper(), "RECRUTER");

            await LoadCharacters();

            foreach (var c in _charactersInfos)
            {
                var trimmedFirstName = c.Firstname.Substring(0, c.Firstname.Length >= 3 ? 3 : 1);
                var trimmedLastName = c.Lastname.Substring(0, c.Lastname.Length >= 3 ? 3 : 1);

                _recruitMenu.AddItem(new MenuButtonItem($"{trimmedFirstName}. {trimmedLastName}.", async item =>
                {
                    _recruitEmployeeMenu = new MenuContainer(_currentEnterprise.JobLabel.ToUpper(), c.Firstname.ToUpper() + " " + c.Lastname.ToUpper());

                    var cJob = _enterprises.Values.ToList().Find(x => x.JobName == c.Job.Name);

                    var jobLabel = "";

                    if (cJob == null)
                    {
                        jobLabel = "Sans emploi";
                    }
                    else
                    {
                        jobLabel = cJob.JobLabel;
                    }

                    var cRole = _currentEnterprise.Roles.Find(x => x.Name == c.Job.Role.Name);

                    var jobRole = "";

                    if (cRole == null)
                    {
                        jobRole = "Aucun grade";
                    }
                    else
                    {
                        jobRole = cRole.Label;
                    }

                    _recruitEmployeeMenu.AddItem(new MenuButtonItem($"Métier: {jobLabel}"));
                    _recruitEmployeeMenu.AddItem(new MenuButtonItem($"Grade: {jobRole}"));

                    var haveGoodJob = _currentEnterprise.JobName == c.Job.Name;
                    _recruitEmployeeMenu.AddItem(new MenuButtonItem("Recruter", async item =>
                    {
                        if (c.Job.Name != _currentEnterprise.JobName)
                        {
                            item.Visible = false;
                            Menu.UpdateRender(_recruitEmployeeMenu, null);
                            var roleLevel = Job.GetRolesByJob(_currentEnterprise.JobName).ToList().Find(x => x.Name == _currentEnterprise.Roles[0].Name).Level;
                            Job.RecruitPlayerByRockstarId(c.RockstarId, _currentEnterprise.JobName, _currentEnterprise.Roles[0].Name, roleLevel);
                            await BaseScript.Delay(1000);

                            InitRecruitMenu();
                            await BaseScript.Delay(100);
                            await Menu.OpenMenu(_employeesManagementMenu);

                            Notification.Schedule("TRAVAIL", $"{c.Firstname} {c.Lastname} est désormais {_currentEnterprise.JobLabel}.", 5000);
                        }
                        else
                        {
                            Notification.Schedule("TRAVAIL", $"{c.Firstname} {c.Lastname} est déjà {_currentEnterprise.JobLabel}.", 5000);
                        }
                    }, !haveGoodJob));

                    await Menu.OpenMenu(_recruitEmployeeMenu);
                }));
            }
        }

        public bool RequirePermissionForMenu(params string[] permissions)
        {
            var job = Character.Current.Job;

            if (!_enterprises.ContainsKey(job.Name)) return false;

            var enterprise = _enterprises[job.Name];

            var role = enterprise?.Roles.Find(x => x.Level == job.Role.Level);
            if (role == null) return false;

            var haveAllNeededPermission = true;

            foreach (var permission in permissions)
            {
                if (!role.Permissions.Contains(permission))
                    haveAllNeededPermission = false;
            }

            return haveAllNeededPermission;
        }
    }
}