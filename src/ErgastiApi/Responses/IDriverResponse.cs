﻿using System.Collections.Generic;
using ErgastApi.Responses.Models;

namespace ErgastApi.Responses
{
    public interface IDriverResponse : IErgastResponse
    {
        IList<IDriver> Drivers { get; }
    }
}