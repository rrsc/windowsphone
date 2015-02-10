﻿using System;
using mega;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, MNode megaNode, object parentCollection = null, object childCollection = null)
            : base(megaSdk, megaNode, parentCollection, childCollection)
        {
            SetFolderInfo();
            this.ThumbnailIsDefaultImage = true;
            this.ThumbnailImageUri = new Uri("folder" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative);
        }

        #region Override Methods

        public override void OpenFile()
        {
            throw new NotSupportedException("Open file is not supported on folder nodes");
        }

        #endregion

        #region Public Methods

        public void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(base.GetMegaNode());
            int childFiles = this.MegaSdk.getNumChildFiles(base.GetMegaNode());
            this.FolderInfo = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder : UiResources.MultipleFolders,
                childFiles, childFiles == 1 ? UiResources.SingleFile : UiResources.MultipleFiles);
        }

        #endregion

        #region Properties

        private string _folderInfo;
        public string FolderInfo
        {
            get { return _folderInfo; }
            private set
            {
                _folderInfo = value;
                OnPropertyChanged("FolderInfo");
            }
        }

        #endregion
    }
}