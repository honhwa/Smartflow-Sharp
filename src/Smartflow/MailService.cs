﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Smartflow
{
    public class MailService : IMailService
    {
        private const string CONST_MAIL_URL_EXPRESSION = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
        private MailConfiguration mailConfiguration = ConfigurationManager.GetSection("mailConfiguration") as MailConfiguration;

        public void Notification(string[] to, string body)
        {
            SmtpClient _smtp = new SmtpClient();
            _smtp.Host = mailConfiguration.Host;
            _smtp.Port = mailConfiguration.Port;
            _smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            _smtp.EnableSsl = mailConfiguration.EnableSsl;
            _smtp.UseDefaultCredentials = true;
            _smtp.Credentials = new NetworkCredential(mailConfiguration.Account, mailConfiguration.Password);
            List<MailMessage> msgList = GetSendMessageList(mailConfiguration.Account, mailConfiguration.Name, to, "待办通知", body);
            foreach (var item in msgList)
            {
                try
                {
                    _smtp.Send(item);
                }
                catch (Exception ex)
                {
                    WorkflowLogger.WriteLog(ex);
                }
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="from">发件人邮件地址</param>
        /// <param name="sender">发件人显示名称</param>
        /// <param name="to">收件人地址</param>
        /// <param name="subject">邮件标题</param>
        /// <param name="body">邮件正文</param>
        protected List<MailMessage> GetSendMessageList(string from, string sender, string[] recvierArray, string subject, string body)
        {
            if (recvierArray.Any(MAddress => !Regex.IsMatch(MAddress, MailService.CONST_MAIL_URL_EXPRESSION)))
                return null;

            List<MailMessage> messageList = new List<MailMessage>();
            foreach (string recvier in recvierArray)
            {
                MailMessage message = new MailMessage(new MailAddress(from, sender), new MailAddress(recvier));
                message.Subject = subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.Body = body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;
                messageList.Add(message);
            }
            return messageList;
        }
    }
}
