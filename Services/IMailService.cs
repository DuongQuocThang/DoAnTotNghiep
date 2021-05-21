using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDoAn.Models;

namespace WebDoAn.Services
{
    public interface IMailService
    {
        public void SendEmail(MailRequest mailRequest);
    }
}
