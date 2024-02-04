using System.Text.Json.Serialization;

namespace Common.Models
{
    public class SessionVariables
    {
        public string DataPath { get; set; }
        public string DbName { get; set; }
        public string ShopUrl { get; set; }
        public string ShopToken { get; set; }
        public string PdfsPath { get; set; }
        public string QrTagsPath { get; set; }
        public string ImagesPath { get; set; }
        public bool IsOnLocalMachine { get; set; } = false;
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string ServerUrl { get; set; }

        public string LoggingEmail { get; set; }
        public string LoggingPassword { get; set; }
        public string BroadcastExchange { get; set; }
        public string BroadcastUsername { get; set; }
        public string BroadcastPassword { get; set; }
        public string BroadcastHost { get; set; }

        public string ShipStationUsername { get; set; }
        public string ShipStationPassword { get; set; }
    }
}
