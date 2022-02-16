using System;

namespace Dapper.MoqTests.Samples
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Car
    {
        public string Registration { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public DateTime? DateRegistered { get; set; }
    }
}
