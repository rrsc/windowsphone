﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    class SettingsViewModel : BaseAppInfoAwareViewModel
    {
        public SettingsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            this.ShareRecoveryKeyCommand = new DelegateCommand(ShareRecoveryKey);
            this.CopyRecoveryKeyCommand = new DelegateCommand(CopyRecoveryKey);
            this.ChangePinLockCommand = new DelegateCommand(ChangePinLock);
            this.ViewRecoveryKeyCommand = new DelegateCommand(ViewRecoveryKey);
            this.CloseOtherSessionsCommand = new DelegateCommand(CloseOtherSessions);

            #if WINDOWS_PHONE_80
            this.SelectDownloadLocationCommand = null;
            #elif WINDOWS_PHONE_81
            this.SelectDownloadLocationCommand = new DelegateCommand(SelectDownloadLocation);
            #endif

            this.MegaSdkCommand = new DelegateCommand(NavigateToMegaSdk);
            this.GoedWareCommand = new DelegateCommand(NavigateToGoedWare);
            this.TermsOfServiceCommand = new DelegateCommand(NavigateToTermsOfService);
            this.PrivacyPolicyCommand = new DelegateCommand(NavigateToPrivacyPolicy);
            this.CopyrightCommand = new DelegateCommand(NavigateToCopyright);
            this.TakedownGuidanceCommand = new DelegateCommand(NavigateToTakedownGuidance);
            this.GeneralCommand = new DelegateCommand(NavigateToGeneral);
            this.DataProtectionRegulationCommand =
                new DelegateCommand(NavigateToDataProtectionRegulation);

            this.Initialize();

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.Settings);
        }

        #region Commands

        public ICommand ShareRecoveryKeyCommand { get; set; }
        public ICommand CopyRecoveryKeyCommand { get; private set; }
        public ICommand ViewRecoveryKeyCommand { get; private set; }
        public ICommand ChangePinLockCommand { get; private set; }
        public ICommand SelectDownloadLocationCommand { get; private set; }
        public ICommand MegaSdkCommand { get; private set; }
        public ICommand GoedWareCommand { get; private set; }
        public ICommand TermsOfServiceCommand { get; private set; }
        public ICommand PrivacyPolicyCommand { get; private set; }
        public ICommand CopyrightCommand { get; private set; }
        public ICommand TakedownGuidanceCommand { get; private set; }
        public ICommand GeneralCommand { get; private set; }
        public ICommand DataProtectionRegulationCommand { get; private set; }
        public ICommand CloseOtherSessionsCommand { get; private set; }

        #endregion

        #region Methods

        private async void Initialize()
        {
            this.AppVersion = AppService.GetAppVersion();
            this.MegaSdkVersion = AppService.GetMegaSDK_Version();

            this.PinLockIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled, false);

            // Do not set the property on initialize, because it fill fire the SetAutoCameraUploadStatus
            _cameraUploadsIsEnabled = MediaService.GetAutoCameraUploadStatus();
            this.CameraUploadsIsEnabledText = _cameraUploadsIsEnabled ? UiResources.On : UiResources.Off;

            #if WINDOWS_PHONE_80
            this.ExportIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.ExportImagesToPhotoAlbum, false);
            #elif WINDOWS_PHONE_81
            this.AskDownloadLocationIsEnabled = SettingsService.LoadSetting<bool>(SettingsResources.AskDownloadLocationIsEnabled, false);
            this.StandardDownloadLocation = SettingsService.LoadSetting<string>(
                SettingsResources.DefaultDownloadLocation, UiResources.DefaultDownloadLocation);
            #endif

            var mfaStatus = await AccountService.CheckMultiFactorAuthStatusAsync();
            switch (mfaStatus)
            {
                case MultiFactorAuthStatus.Enabled:
                    this.IsMultiFactorAuthAvailable = true;
                    SetField(ref this._isMultiFactorAuthEnabled, true, "IsMultiFactorAuthEnabled");
                    break;

                case MultiFactorAuthStatus.Disabled:
                    this.IsMultiFactorAuthAvailable = true;
                    SetField(ref this._isMultiFactorAuthEnabled, false, "IsMultiFactorAuthEnabled");
                    break;

                case MultiFactorAuthStatus.NotAvailable:
                    this.IsMultiFactorAuthAvailable = false;
                    break;
            }
        }

        private void ShareRecoveryKey(object obj)
        {
            var shareStatusTask = new ShareStatusTask {Status = MegaSdk.exportMasterKey()};
            shareStatusTask.Show();
        }

        private void CopyRecoveryKey(object obj)
        {
            CopyClipboard();
        }

        private void CopyClipboard()
        {
            try
            {
                Clipboard.SetText(MegaSdk.exportMasterKey());
                MegaSdk.masterKeyExported();
                new CustomMessageDialog(
                           AppMessages.AM_RecoveryKeyCopied_Title,
                           AppMessages.AM_RecoveryKeyCopied,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                           AppMessages.AM_RecoveryKeyClipboardFailed_Title,
                           AppMessages.AM_RecoveryKeyClipboardFailed,
                           App.AppInformation,
                           MessageDialogButtons.Ok).ShowDialog();
            }
        }
        
        private void ViewRecoveryKey(object obj)
        {
            DialogService.ShowViewRecoveryKey(MegaSdk.exportMasterKey(), CopyClipboard);
        }

        public void ProcessBackupLink()
        {
            DialogService.ShowViewRecoveryKey(MegaSdk.exportMasterKey(), CopyClipboard);
        }

        private void ChangePinLock(object obj)
        {
            DialogService.ShowPinLockDialog(true, this);
        }

        #if WINDOWS_PHONE_81
        private void SelectDownloadLocation(object obj)
        {
            if (App.FileOpenOrFolderPickerOpenend) return;
            FolderService.SelectFolder("SelectDefaultDownloadFolder");
        }
        #endif

        private void NavigateToMegaSdk(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_SdkLink) };
            webBrowserTask.Show();
        }

        private void NavigateToGoedWare(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.GoedWareUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToTermsOfService(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_TermsOfServiceUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToPrivacyPolicy(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_PrivacyPolicyUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToCopyright(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_CopyrightUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToTakedownGuidance(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_TakedownGuidanceUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToGeneral(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_GeneralLegalUrl) };
            webBrowserTask.Show();
        }

        private void NavigateToDataProtectionRegulation(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_DataProtectionRegulationUrl) };
            webBrowserTask.Show();
        }

        private async void CloseOtherSessions(object obj)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            var result = await new CustomMessageDialog(
                UiResources.UI_Warning,
                AppMessages.AM_CloseOtherSessionsQuestionMessage,
                App.AppInformation,
                MessageDialogButtons.YesNo).ShowDialogAsync();

            if (result == MessageDialogResult.CancelNo) return;

            this.MegaSdk.killAllSessions(new KillAllSessionsRequestListener());
        }

        /// <summary>
        /// Enable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> EnableMultiFactorAuthAsync()
        {
            return await DialogService.ShowMultiFactorAuthSetupDialogAsync();
        }

        /// <summary>
        /// Show the dialog to disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> ShowDisableMultiFactorAuthDialogAsync()
        {
            var result = await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(
                this.DisableMultiFactorAuthAsync,
                AppMessages.AM_2FA_DisableDialogTitle,
                AppMessages.AM_2FA_DisableDialogMessage);

            if (result)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_2FA_DisabledDialogTitle, string.Empty,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                });
            }

            return result;
        }

        /// <summary>
        /// Disable the Multi-Factor Authentication
        /// </summary>
        /// <returns>TRUE if all is OK or FALSE if something failed</returns>
        private async Task<bool> DisableMultiFactorAuthAsync(string code)
        {
            var disableMultiFactorAuth = new MultiFactorAuthDisableRequestListenerAsync();
            var result = await disableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthDisable(code, disableMultiFactorAuth));

            if (!result)
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();

            return result;
        }

        private async void OnIsMultiFactorAuthEnabledValueChanged()
        {
            var value = this.IsMultiFactorAuthEnabled ?
                await this.EnableMultiFactorAuthAsync() :
                !await this.ShowDisableMultiFactorAuthDialogAsync();
            
            SetField(ref this._isMultiFactorAuthEnabled, value, "IsMultiFactorAuthEnabled");
        }

        #endregion

        #region Properties

        public string AppVersion { get; private set; }

        public string MegaSdkVersion { get; private set; }

        private bool _exportIsEnabled;
        public bool ExportIsEnabled
        {
            get { return _exportIsEnabled; }
            set
            {
                if(_exportIsEnabled != value)
                    SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, value);
                
                _exportIsEnabled = value;

                ExportIsEnabledText = _exportIsEnabled ? UiResources.On : UiResources.Off;
                
                OnPropertyChanged("ExportIsEnabled");
            }
        }

        private bool _askDownloadLocationIsEnabled;
        public bool AskDownloadLocationIsEnabled
        {
            get { return _askDownloadLocationIsEnabled; }
            set
            {
                if (_askDownloadLocationIsEnabled != value)
                    SettingsService.SaveSetting(SettingsResources.AskDownloadLocationIsEnabled, value);

                _askDownloadLocationIsEnabled = value;
                DownloadLocationSelectionIsEnabled = !_askDownloadLocationIsEnabled;

                AskDownloadLocationIsEnabledText = _askDownloadLocationIsEnabled ? UiResources.On : UiResources.Off;

                OnPropertyChanged("AskDownloadLocationIsEnabled");
            }
        }

        private bool _downloadLocationSelectionIsEnabled;
        public bool DownloadLocationSelectionIsEnabled
        {
            get { return _downloadLocationSelectionIsEnabled; }
            set
            {
                _downloadLocationSelectionIsEnabled = value;
                OnPropertyChanged("DownloadLocationSelectionIsEnabled");
            }
        }

        private bool _pinLockIsEnabled;
        public bool PinLockIsEnabled
        {
            get { return _pinLockIsEnabled; }
            set
            {
                if (_pinLockIsEnabled && !value)
                {
                    SettingsService.DeleteSetting(SettingsResources.UserPinLockIsEnabled);
                    SettingsService.DeleteSetting(SettingsResources.UserPinLock);
                }

                _pinLockIsEnabled = value;

                PinLockIsEnabledText = _pinLockIsEnabled ? UiResources.On : UiResources.Off;

                if (_pinLockIsEnabled)
                    DialogService.ShowPinLockDialog(false, this);
                
                OnPropertyChanged("PinLockIsEnabled");
            }
        }

        private string _cameraUploadsIsEnabledText;
        public string CameraUploadsIsEnabledText
        {
            get { return _cameraUploadsIsEnabledText; }
            set
            {
                _cameraUploadsIsEnabledText = value;
                OnPropertyChanged("CameraUploadsIsEnabledText");
            }
        }

        private bool _cameraUploadsIsEnabled;
        public bool CameraUploadsIsEnabled
        {
            get { return _cameraUploadsIsEnabled; }
            set
            {
                if (_cameraUploadsIsEnabled != value)
                {
                    SettingsService.SaveSetting(SettingsResources.CameraUploadsIsEnabled, value);
                    if(!value) SettingsService.DeleteFileSetting("LastUploadDate");
                }

                _cameraUploadsIsEnabled = MediaService.SetAutoCameraUpload(value);
                this.CameraUploadsIsEnabledText = _cameraUploadsIsEnabled ? UiResources.On : UiResources.Off;
                OnPropertyChanged("CameraUploadsIsEnabled");
            }
        }

        private string _pinLockIsEnabledText;
        public string PinLockIsEnabledText
        {
            get { return _pinLockIsEnabledText; }
            set
            {
                _pinLockIsEnabledText = value;
                OnPropertyChanged("PinLockIsEnabledText");
            }
        }

        private string _exportIsEnabledText;
        public string ExportIsEnabledText
        {
            get { return _exportIsEnabledText; }
            set
            {
                _exportIsEnabledText = value;
                OnPropertyChanged("ExportIsEnabledText");
            }
        }

        private string _askDownloadLocationIsEnabledText;
        public string AskDownloadLocationIsEnabledText
        {
            get { return _askDownloadLocationIsEnabledText; }
            set
            {
                _askDownloadLocationIsEnabledText = value;
                OnPropertyChanged("AskDownloadLocationIsEnabledText");
            }
        }

        private string _standardDownloadLocation;
        public string StandardDownloadLocation
        {
            get { return _standardDownloadLocation; }
            set
            {
                _standardDownloadLocation = value;
                OnPropertyChanged("StandardDownloadLocation");
            }
        }

        private bool _isMultiFactorAuthEnabled;
        public bool IsMultiFactorAuthEnabled
        {
            get { return _isMultiFactorAuthEnabled; }
            set
            {
                if (!SetField(ref _isMultiFactorAuthEnabled, value))
                    return;

                OnIsMultiFactorAuthEnabledValueChanged();
            }
        }

        private bool _isMultiFactorAuthAvailable;
        public bool IsMultiFactorAuthAvailable
        {
            get { return _isMultiFactorAuthAvailable; }
            set { SetField(ref _isMultiFactorAuthAvailable, value); }
        }

        #endregion
    }
}
