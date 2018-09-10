﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Aiursoft.Pylon.Services
{
    public class AiurEmailSender
    {
        private readonly IConfiguration _configuration;

        public AiurEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmail(string target, string subject, string content)
        {
            var client = new SmtpClient("42.120.226.4")
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("service@aiursoft.com", _configuration["Emailpassword"])
            };
            return client.SendMailAsync("service@aiursoft.com", target, subject, content);
        }
    }
}
