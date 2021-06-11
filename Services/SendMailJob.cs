using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDoAn.Interfaces;
using WebDoAn.Models;

namespace WebDoAn.Services
{
    [DisallowConcurrentExecution]
    public class SendMailJob : IJob
    {
        private readonly IWaterService _waterService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        public SendMailJob(IWaterService ws, IMailService mailService, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _waterService = ws;
            _mailService = mailService;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var adminUser = await _userManager.GetUsersInRoleAsync("Administrators");
            var SupUser = await _userManager.GetUsersInRoleAsync("Supervisor");
            List<string> email = new List<string>();
            foreach (var admin in adminUser)
            {
                email.Add(admin.Email);
            }

            foreach (var sup in SupUser)
            {
                email.Add(sup.Email);
            }


            var file = new Attachment
            {
                fileName = $"Thống kê {DateTime.Today.ToString("dd-MM-yyyy")}.csv",
                Data = StatisticalCal()
            };

            var mailResquest = new MailRequest
            {
                ToEmails = email,
                Body = $"Thống kê quản lý nguồn nước ngày {DateTime.Today.ToString("dd/MM/yyyy")}",
                Subject = $"Thống kê quản lý nguồn nước ngày {DateTime.Today.ToString("dd/MM/yyyy")}"
            };
            mailResquest.Attachments = new List<Attachment>();
            mailResquest.Attachments.Add(file);
            _mailService.SendEmail(mailResquest);

        }

        public string StatisticalCal()
        {
            var listWater = _waterService.GetWater(DateTime.Today, DateTime.Today).ToList();
            List<Warning> warnings = new List<Warning>();
            double TotalWaterflow = 0;
            foreach (var water in listWater)
            {
                TotalWaterflow += double.Parse(water.Waterflow);
            }

            for (int i = 0; i < listWater.Count; i++)
            {
                if (int.Parse(listWater[i].PH) >= 9)
                {
                    Warning warningItem = new Warning
                    {
                        startTime = listWater[i].Time,
                        endTime = listWater[i].Time,
                        Message = "pH >=9 Chỉ số vượt quá mức quy định"
                    };
                    for (int j = i + 1; j < listWater.Count; j++)
                    {
                        if (int.Parse(listWater[j].PH) < 9 || j == listWater.Count - 1)
                        {
                            warningItem.endTime = listWater[j - 1].Time;
                            i = j - 1;
                            break;
                        }
                    }
                    warnings.Add(warningItem);
                }
                else if (int.Parse(listWater[i].PH) <= 6)
                {
                    Warning warningItem = new Warning
                    {
                        startTime = listWater[i].Time,
                        endTime = listWater[i].Time,
                        Message = "pH <= 6 Chỉ số vượt tiêu chuẩn cho phép"
                    };
                    for (int j = i + 1; j < listWater.Count; j++)
                    {
                        if (int.Parse(listWater[j].PH) > 6 || j == listWater.Count - 1)
                        {
                            warningItem.endTime = listWater[j - 1].Time;
                            i = j - 1;
                            break;
                        }
                    }
                    warnings.Add(warningItem);

                }
            }

            var buildler = new System.Text.StringBuilder();
            buildler.AppendLine("startTime,endTime,Message");
            foreach (var warning in warnings)
            {
                buildler.AppendLine($"{warning.startTime.ToString("dd/MM/yyyy HH:mm:ss")},{warning.endTime.ToString("dd/MM/yyyy HH:mm:ss")},{warning.Message}");
            }
            buildler.AppendLine($"Waterflow total : {TotalWaterflow}");
            return buildler.ToString();
        }
    }
}
