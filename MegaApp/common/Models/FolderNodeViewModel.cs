﻿using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Resources;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using MegaApp.Extensions;
using Telerik.Windows.Controls;

namespace MegaApp.Models
{
    public class FolderNodeViewModel: NodeViewModel
    {
        public FolderNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentCollection, childCollection)
        {
            SetFolderInfo();
            
            this.IsDefaultImage = true;
            this.DefaultImagePathData = VisualResources.FolderTypePath_default;

            if (!megaNode.getName().ToLower().Equals("camera uploads")) return;
            this.DefaultImagePathData = VisualResources.FolderTypePath_photo;
        }

        #region Override Methods

        public override void Open()
        {
            throw new NotSupportedException("Open file is not supported on folder nodes");
        }

       
        #endregion

        #region Public Methods

        public void SetFolderInfo()
        {
            int childFolders = this.MegaSdk.getNumChildFolders(this.OriginalMNode);
            int childFiles = this.MegaSdk.getNumChildFiles(this.OriginalMNode);
            this.Information = String.Format("{0} {1} | {2} {3}",
                childFolders, childFolders == 1 ? UiResources.SingleFolder.ToLower() : UiResources.MultipleFolders.ToLower(),
                childFiles, childFiles == 1 ? UiResources.SingleFile.ToLower() : UiResources.MultipleFiles.ToLower());
        }

        public void CreateShortCut()
        {
            var iconPath = new Path()
            {
                Height = 150,
                Width = 150,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = new SolidColorBrush(Colors.White)
            };

            iconPath.SetDataBinding(this.DefaultImagePathData);

            var shortCutTile = new RadIconicTileData()
            {
                IconVisualElement = iconPath,
                SmallIconVisualElement = iconPath,
                MeasureMode = MeasureMode.Tile,
                Title = this.Name
            };

            LiveTileHelper.CreateOrUpdateTile(shortCutTile,
                new Uri("/Pages/MainPage.xaml?ShortCutHandle=" + this.OriginalMNode.getHandle(), UriKind.Relative),
                false);
        }

        #endregion
    }
}
