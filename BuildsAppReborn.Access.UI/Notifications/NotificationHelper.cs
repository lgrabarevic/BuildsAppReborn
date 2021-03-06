﻿using System;
using BuildsAppReborn.Contracts.Models;

namespace BuildsAppReborn.Access.UI.Notifications
{
    internal static class NotificationHelper
    {
        public static String GenerateStatus(this IBuild build)
        {
            return build.Status.ToString();
        }

        public static String GenerateTitle(this IBuild build, BuildViewStyle viewStyle)
        {
            if (viewStyle == BuildViewStyle.GroupByPullRequest)
            {
                if (build.PullRequest != null)
                {
                    return $"{build.Definition.Project.Name} - {build.PullRequest.Title} - {build.BuildNumber}";
                }
            }

            return $"{build.Definition.Project.Name} - {build.Definition.Name} - {build.BuildNumber}";
        }

        public static String GenerateUsername(this IBuild build)
        {
            return build.DisplayUser.DisplayName;
        }
    }
}