﻿using System;
using System.Diagnostics.CodeAnalysis;
using BuildsAppReborn.Contracts.Models;
using BuildsAppReborn.Infrastructure;

namespace BuildsAppReborn.Contracts.UI
{
    public class BuildItem : ViewModelBase
    {
        #region Constructors

        public BuildItem(IBuild build)
        {
            Build = build;
        }

        #endregion

        #region Public Properties

        public IBuild Build { get; }

        public TimeSpan BuildDuration => BuildEndTime - BuildStartTime;

        public DateTime BuildEndTime
        {
            get
            {
                switch (Build.Status)
                {
                    case BuildStatus.Queued:
                    case BuildStatus.Running:
                    case BuildStatus.Unknown:
                        return DateTime.UtcNow;
                    default:
                        return Build.FinishDateTime;
                }
            }
        }

        public DateTime BuildStartTime
        {
            get
            {
                switch (Build.Status)
                {
                    case BuildStatus.Queued:
                    case BuildStatus.Unknown:
                        return Build.QueueDateTime;
                    default:
                        return Build.StartDateTime;
                }
            }
        }

        public DateTime BuildStateTime
        {
            get
            {
                switch (Build.Status)
                {
                    case BuildStatus.Unknown:
                        return Build.QueueDateTime;
                    case BuildStatus.Succeeded:
                    case BuildStatus.PartiallySucceeded:
                    case BuildStatus.Failed:
                    case BuildStatus.Stopped:
                        return Build.FinishDateTime;
                    case BuildStatus.Running:
                        return Build.StartDateTime;
                    case BuildStatus.Queued:
                    default:
                        return Build.QueueDateTime;
                }
            }
        }

        public BuildStatus BuildStatus => Build?.Status ?? BuildStatus.Unknown;

        public String Comment => Build?.SourceVersion?.Comment ?? "-";

        #endregion

        #region Public Methods

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        public void Refresh()
        {
            OnPropertyChanged(nameof(BuildStartTime));
            OnPropertyChanged(nameof(BuildEndTime));
            OnPropertyChanged(nameof(BuildStateTime));
            OnPropertyChanged(nameof(BuildDuration));
            OnPropertyChanged(nameof(BuildStatus));
        }

        #endregion
    }
}