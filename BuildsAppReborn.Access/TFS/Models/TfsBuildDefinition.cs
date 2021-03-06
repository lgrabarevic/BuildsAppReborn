﻿using System;
using BuildsAppReborn.Contracts.Models;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BuildsAppReborn.Access.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal abstract class TfsBuildDefinition : IBuildDefinition
    {
        [JsonIgnore]
        public String BuildSettingsId { get; internal set; }

        [JsonProperty("id")]
        public Int32 Id { get; private set; }

        [JsonProperty("name")]
        public String Name { get; private set; }

        [JsonProperty("project")]
        public abstract IProject Project { get; protected set; }

        [JsonProperty("type")]
        public String Type { get; private set; }

        [JsonProperty("url")]
        public String Url { get; private set; }
    }
}