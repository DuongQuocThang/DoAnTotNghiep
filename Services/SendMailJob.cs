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
        public SendMailJob(IWaterService ws, IMailService mailService, IConfiguration configuration)
        {
            _waterService = ws;
            _mailService = mailService;
            _configuration = configuration;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var file = new Attachment
            {
                fileName = $"Statistical{DateTime.Today.ToString("dd-MM-yyyy")}.csv",
                Data = StatisticalCal()
            };

            var mailResquest = new MailRequest
            {
                ToEmail = _configuration["ToMail"].ToString(),
                Body = $"Thống kê quản lý nguồn nước ngày {DateTime.Today.ToString("dd/MM/yyyy")}",
                Subject = $"Thống kê quản lý nguồn nước ngày {DateTime.Today.ToString("dd/MM/yyyy")}"
            };
            mailResquest.Attachments = new List<Attachment>();
            mailResquest.Attachments.Add(file);
            _mailService.SendEmail(mailResquest);

            return Task.CompletedTask;
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
                if (int.Parse(listWater[i].PH) >= 8)
                {
                    Warning warningItem = new Warning
                    {
                        startTime = listWater[i].Time,
                        endTime = listWater[i].Time,
                        Message = "pH >=8 Chỉ số vượt quá mức quy định"
                    };
                    for (int j = i + 1; j < listWater.Count; j++)
                    {
                        if (int.Parse(listWater[j].PH) < 8 || j == listWater.Count - 1)
                        {
                            warningItem.endTime = listWater[j - 1].Time;
                            i = j - 1;
                            break;
                        }
                    }
                    warnings.Add(warningItem);
                }
                else if (int.Parse(listWater[i].PH) <= 4)
                {
                    Warning warningItem = new Warning
                    {
                        startTime = listWater[i].Time,
                        endTime = listWater[i].Time,
                        Message = "pH <= 4 Chỉ số vượt tiêu chuẩn cho phép"
                    };
                    for (int j = i + 1; j < listWater.Count; j++)
                    {
                        if (int.Parse(listWater[j].PH) > 4 || j == listWater.Count - 1)
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
