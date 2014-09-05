﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cineworld
{

    public partial class Person
    {
        public class Crew2
        {

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("original_title")]
            public string OriginalTitle { get; set; }

            [JsonProperty("department")]
            public string Department { get; set; }

            [JsonProperty("job")]
            public string Job { get; set; }

            [JsonProperty("poster_path")]
            public string PosterPath { get; set; }

            [JsonProperty("release_date")]
            public string ReleaseDate { get; set; }

            [JsonProperty("adult")]
            public bool Adult { get; set; }
        }
    }

}
