﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebSocketLibrary.Schemas
{
    public class CpuMode
    {
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }
        [JsonPropertyName("modecode")]
        public int Modecode { get; set; }
    }
}
