using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreAuthKit.ApiKey.Models
{
    public class ApiKeyOptions
    {
        public string HeaderName { get; set; } = "X-Api-Key";

        public string? QueryParamName { get; set; } = null;

        public string[] ValidKeys { get; set; } = [];

        public Func<string, Task<bool>>? ValidateKeyAsync { get; set; } = null;
    }
}
