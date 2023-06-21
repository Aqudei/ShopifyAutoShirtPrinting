namespace ShopifyEasyShirtPrinting.Models
{

    public class SettingItem
    {
        public enum SettingItemType
        {
            Environment = 0,
            App
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public SettingItemType SettingType { get; set; } = SettingItemType.Environment;

        public SettingItem()
        {

        }
    }
}
