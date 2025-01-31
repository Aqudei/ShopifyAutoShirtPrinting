using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Common.Models
{
    public class LineItemViewModel : BindableBase
    {
        private bool _isSelected;

        public int Id { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }


        public string Sku { get => _sku; set => SetProperty(ref _sku, value); }


        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public long? VariantId { get; set; }

        public string VariantTitle { get; set; }

        public long? LineItemId { get; set; }

        public int? Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        public string FulfillmentStatus { get; set; }

        public string FinancialStatus { get; set; }

        public string Customer { get; set; }

        public string CustomerEmail { get; set; }


        public DateTime? DateModified
        {
            get => _dateModified; set
            {
                SetProperty(ref _dateModified, value);
                RaisePropertyChanged(nameof(DateModifiedLocal));
            }
        }

        public DateTime? DateModifiedLocal
        {
            get
            {
                if (DateModified.HasValue)
                {
                    return DateModified.Value.ToUniversalTime().ToLocalTime();
                }

                return null;
            }
        }

        public string ProductImage { get; set; }

        public string Notes
        {
            get => _notes;
            set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(nameof(HasNotes));
            }
        }


        public long? OrderId { get; set; }

        private int _printedQuantity;
        private int? _binNumber;
        private string _status = "Pending";
        private string _notes;
        private DateTime? _dateModified;
        private int? _quantity;
        private string _orderNumber;
        private string _name;
        private int _originalBinNumber;
        private string _sku;
        private string _shipping;

        public int PrintedQuantity
        {
            get => _printedQuantity;
            set => SetProperty(ref _printedQuantity, value);
        }

        public int? BinNumber
        {
            get => _binNumber;
            set => SetProperty(ref _binNumber, value);
        }

        public int OriginalBinNumber { get => _originalBinNumber; set => SetProperty(ref _originalBinNumber, value); }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        public string Shipping { get => _shipping; set => SetProperty(ref _shipping, value); }
        public bool ForPickup { get; set; }

        public decimal Grams { get; set; }

        public decimal Length { get; set; }

        public decimal Width { get; set; }
        public decimal Height { get; set; }

    }
}