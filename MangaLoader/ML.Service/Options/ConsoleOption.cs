using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Service.Options
{
    public class ConsoleOption
    {
        [Option('l', "login", HelpText = "Login for authorization", Required = true)]
        public string Login { get; set; }

        [Option('p', "password", HelpText = "Password for authorization", Required = true)]
        public string Password { get; set; }

        [Option('u', "url", HelpText = "Url manga", Required = true)]
        public string MangaLink { get; set; }
    }
}
