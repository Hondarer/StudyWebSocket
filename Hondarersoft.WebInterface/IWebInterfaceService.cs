﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hondarersoft.WebInterface
{
    public interface IWebInterfaceService : IWebInterface
    {
        public Task StartAsync();
    }
}
