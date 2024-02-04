using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintAgent.Models
{
    public class Sku
    {
        public string ProductType { get; set; }
        public string Color { get; set; }
        public string Fit { get; set; }
        public string Style { get; set; }
        public string Size { get; set; }
        public static Sku Parse(string skuText)
        {
            try
            {
                var skuParts = skuText.Split('-');
                var sku = new Sku
                {
                    ProductType = skuParts[0],
                    Fit = skuParts[3],
                    Size = skuParts[4],
                };

                if (skuParts.Length > 5)
                {
                    sku.Color = skuParts[5];
                }
                return sku;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
