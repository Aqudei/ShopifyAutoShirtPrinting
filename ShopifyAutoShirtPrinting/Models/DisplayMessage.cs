using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Models
{
    public class DisplayMessage : BindableBase
    {
        private string message;
        private MESSAGE_TYPE messageType;

        public enum MESSAGE_TYPE
        {
            Info,
            Warning,
            Error,
            Success
        }

        public DisplayMessage()
        {
            Message = string.Empty;
            MessageType = MESSAGE_TYPE.Info;
        }
        public DisplayMessage(string message, MESSAGE_TYPE messageType)
        {
            Message = message;
            MessageType = messageType;
        }
        public DisplayMessage(string message)
        {
            Message = message;
            MessageType = MESSAGE_TYPE.Info;
        }
        public string Message { get => message; set => SetProperty(ref message, value); }
        public MESSAGE_TYPE MessageType { get => messageType; set => SetProperty(ref messageType, value); }

    }
}
