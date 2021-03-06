﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BuildsAppReborn.Client.Controls;
using BuildsAppReborn.Client.Resources;
using BuildsAppReborn.Contracts.Models;

namespace BuildsAppReborn.Client.Converter
{
    public class TestRunToStackedItemsConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var testRun = value as ITestRun;
            if (testRun == null)
            {
                return Binding.DoNothing;
            }

            var stackedItems = new List<StackedItem>();

            stackedItems.Add(new StackedItem {Value = testRun.FailedTests, Title = "Failed Tests", Color = Brushes.Red, Icon = IconProvider.FailIcon});
            stackedItems.Add(new StackedItem {Value = testRun.NotApplicableTests, Title = "Not Executed Tests", Color = Brushes.Yellow, Icon = IconProvider.UnknownIcon});
            stackedItems.Add(new StackedItem {Value = testRun.PassedTests, Title = "Passed Tests", Color = Brushes.Green, Icon = IconProvider.SuccessIcon});
            stackedItems.Add(new StackedItem {Value = testRun.IncompleteTests, Title = "Incomplete Tests", Color = Brushes.Blue, Icon = IconProvider.LoadingIcon});

            return new StackedItemCollection(stackedItems) {TotalCount = testRun.TotalTests, Title = "Total Tests"};
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}