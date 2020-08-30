﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hondarersoft.WebInterface
{
    public interface IWebInterfaceService
    {
        public string Hostname { get; set; }

        public int PortNumber { get; set; }

        public string BasePath { get; set; }

        public bool UseSSL { get; set; }

        public Task StartAsync();
    }
}
