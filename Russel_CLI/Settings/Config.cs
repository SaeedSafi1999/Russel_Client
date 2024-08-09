using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Russel_CLI.Settings
{
    public class ApiSettings
    {
        public string Port { get; set; }
    }
    public class Config
    {
        public ApiSettings ApiSettings { get; set; }
    }
}
