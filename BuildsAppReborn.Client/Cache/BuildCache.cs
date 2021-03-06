﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using BuildsAppReborn.Client.Resources;
using BuildsAppReborn.Contracts;
using BuildsAppReborn.Contracts.Models;
using BuildsAppReborn.Contracts.UI;
using BuildsAppReborn.Infrastructure;
using BuildsAppReborn.Infrastructure.Collections;

namespace BuildsAppReborn.Client
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildCache : ViewModelBase
    {
        [ImportingConstructor]
        internal BuildCache(IBuildMonitorBasic buildMonitor,
                            IEqualityComparer<IBuildDefinition> buildDefinitionEqualityComparer,
                            IEqualityComparer<IPullRequest> pullRequstEqualityComparer,
                            GeneralSettings generalSettings)
        {
            BuildsStatus = new RangeObservableCollection<BuildStatusGroup>();
            this.buildDefinitionEqualityComparer = buildDefinitionEqualityComparer;
            this.pullRequstEqualityComparer = pullRequstEqualityComparer;
            this.generalSettings = generalSettings;
            this.buildMonitor = buildMonitor;
            buildMonitor.BuildsUpdated += OnBuildsUpdated;
            buildMonitor.MonitorStopped += (sender, args) => CacheStatus = BuildCacheStatus.NotConfigured;
            buildMonitor.MonitorStarted += (sender, args) => CacheStatus = BuildCacheStatus.Loading;

            if (buildMonitor.IsConfigured)
            {
                CacheStatus = BuildCacheStatus.Loading;
                buildMonitor.BeginPollingBuildsAsync();
            }
            else
            {
                CacheStatus = BuildCacheStatus.NotConfigured;
            }

            UpdateCurrentIcon();
        }

        public RangeObservableCollection<BuildStatusGroup> BuildsStatus { get; }

        public BuildCacheStatus CacheStatus
        {
            get { return this.cacheStatus; }
            private set
            {
                this.cacheStatus = value;
                OnPropertyChanged();
            }
        }

        public String CurrentIcon
        {
            get { return this.currentIcon; }
            set
            {
                if (this.currentIcon != value)
                {
                    this.currentIcon = value;
                    OnPropertyChanged();
                }
            }
        }

        public IProgressComponent ProgressComponent => this.buildMonitor;

        public event EventHandler CacheUpdated;

        protected virtual void OnCacheUpdated()
        {
            CacheUpdated?.Invoke(this, EventArgs.Empty);
        }

        private List<BuildStatusGroup> GroupBuildsByDefinition(IEnumerable<IBuild> builds, ICollection<BuildStatusGroup> currentBuildsStatus)
        {
            currentBuildsStatus = currentBuildsStatus.Where(a => a.BuildDefinition != null).OrderBy(a => a.BuildDefinition.Name).ToList();

            var buildStatusGroups = new List<BuildStatusGroup>();
            var groupByDefinition = builds.GroupBy(a => a.Definition, build => build, this.buildDefinitionEqualityComparer);
            foreach (var grp in groupByDefinition)
            {
                var oldStatus = currentBuildsStatus.SingleOrDefault(a => this.buildDefinitionEqualityComparer.Equals(grp.Key, a.BuildDefinition));
                var newStatus = new BuildStatusGroup(grp.Key, grp.Select(a => new BuildItem(a, BuildViewStyle.GroupByBuildDefinition)).ToList());

                OnBuildStatusChanged(oldStatus, newStatus);

                buildStatusGroups.Add(newStatus);
            }

            return buildStatusGroups;
        }

        private List<BuildStatusGroup> GroupBuildsByPullRequest(List<IBuild> builds, ICollection<BuildStatusGroup> currentBuildsStatus)
        {
            currentBuildsStatus = currentBuildsStatus.Where(a => a.PullRequest != null).OrderBy(a => a.PullRequest.Title).ToList();

            var buildStatusGroups = new List<BuildStatusGroup>();
            var groupByPr = builds.GroupBy(a => a.PullRequest, build => build, this.pullRequstEqualityComparer);
            foreach (var grp in groupByPr)
            {
                var oldStatus = currentBuildsStatus.SingleOrDefault(a => this.pullRequstEqualityComparer.Equals(grp.Key, a.PullRequest));
                var newStatus = new BuildStatusGroup(grp.Key, grp.Select(a => new BuildItem(a, BuildViewStyle.GroupByPullRequest)).ToList());

                OnBuildStatusChanged(oldStatus, newStatus);

                buildStatusGroups.Add(newStatus);
            }

            return buildStatusGroups;
        }

        private void OnBuildStatusChanged(BuildStatusGroup oldStatus, BuildStatusGroup newStatus)
        {
            if (oldStatus != null)
            {
                // ToDo: implement proper update of bound viewmodel objects instead of creating new ones everytime
                newStatus.AdditionalInformationShown = oldStatus.AdditionalInformationShown;
            }
        }

        private void OnBuildsUpdated(ICollection<IBuild> builds)
        {
            if (!builds.Any())
            {
                return;
            }

            var buildStatusGroups = new List<BuildStatusGroup>();

            if (this.generalSettings.ViewStyle == BuildViewStyle.GroupByBuildDefinition)
            {
                buildStatusGroups.AddRange(GroupBuildsByDefinition(builds, BuildsStatus));
            }
            else if (this.generalSettings.ViewStyle == BuildViewStyle.GroupByPullRequest)
            {
                var prBuilds = builds.Where(a => a.PullRequest != null).ToList();

                var groupedBuildsByPullRequest = GroupBuildsByPullRequest(prBuilds, BuildsStatus);
                var groupedBuildsByDefinition = GroupBuildsByDefinition(builds.Except(prBuilds), BuildsStatus);

                buildStatusGroups.AddRange(groupedBuildsByPullRequest);
                buildStatusGroups.AddRange(groupedBuildsByDefinition);
            }

            //buildStatusGroups = buildStatusGroups.OrderByDescending(a => a.CurrentBuild.BuildStartTime).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                BuildsStatus.Clear();
                BuildsStatus.AddRange(buildStatusGroups);
                CacheStatus = BuildCacheStatus.Operational;
            });
            UpdateCurrentIcon();
            OnCacheUpdated();
        }

        private void UpdateCurrentIcon()
        {
            if (CacheStatus == BuildCacheStatus.Loading)
            {
                CurrentIcon = IconProvider.LoadingIcon;
            }
            else if (CacheStatus == BuildCacheStatus.NotConfigured)
            {
                CurrentIcon = IconProvider.SettingsIcon;
            }
            else if (CacheStatus == BuildCacheStatus.Operational)
            {
                if (!BuildsStatus.Any())
                {
                    return;
                }

                var relevantBuilds = new List<IBuild>();
                foreach (var buildStatus in BuildsStatus)
                {
                    var lastFinishedBuild = buildStatus.AllBuildItems.LastOrDefault(a => a.Build.Status != BuildStatus.Running && a.Build.Status != BuildStatus.Queued);
                    if (lastFinishedBuild != null)
                    {
                        relevantBuilds.Add(lastFinishedBuild.Build);
                    }
                }

                if (relevantBuilds.All(a => a.Status == BuildStatus.Succeeded))
                {
                    CurrentIcon = IconProvider.GetIconForBuildStatus(BuildStatus.Succeeded);
                    return;
                }

                if (relevantBuilds.Any(a => a.Status == BuildStatus.PartiallySucceeded))
                {
                    CurrentIcon = IconProvider.GetIconForBuildStatus(BuildStatus.PartiallySucceeded);
                    return;
                }

                if (relevantBuilds.Any(a => a.Status == BuildStatus.Failed))
                {
                    CurrentIcon = IconProvider.GetIconForBuildStatus(BuildStatus.Failed);
                }
            }
        }

        private readonly IEqualityComparer<IBuildDefinition> buildDefinitionEqualityComparer;
        private readonly IBuildMonitorBasic buildMonitor;
        private BuildCacheStatus cacheStatus;
        private String currentIcon;
        private readonly GeneralSettings generalSettings;
        private readonly IEqualityComparer<IPullRequest> pullRequstEqualityComparer;
    }
}