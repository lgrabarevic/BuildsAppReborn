﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BuildsAppReborn.Contracts.Models;

namespace BuildsAppReborn.Contracts
{
    public interface IBuildMonitorBasic
    {
        #region Public Properties

        Boolean IsConfigured { get; }

        #endregion

        #region Public Methods

        Task BeginPollingBuilds();

        #endregion

        event BuildsUpdatedEventHandler BuildsUpdated;

        event EventHandler MonitorStopped;

        event EventHandler MonitorStarted;
    }

    public delegate void BuildsUpdatedEventHandler(ICollection<IBuild> builds);
}