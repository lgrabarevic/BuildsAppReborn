﻿using System;
using System.Windows.Controls;
using BuildsAppReborn.Access.UI.Resources;
using BuildsAppReborn.Contracts.UI;

namespace BuildsAppReborn.Access.UI.View
{
    internal abstract class BuildProviderViewBase : UserControl, IBuildProviderView
    {
        public String Header => Resource.TfsConnectBoxLabel;

        public virtual IBuildProviderViewModel ViewModel
        {
            get { return (IBuildProviderViewModel) DataContext; }
            protected set { DataContext = value; }
        }
    }
}