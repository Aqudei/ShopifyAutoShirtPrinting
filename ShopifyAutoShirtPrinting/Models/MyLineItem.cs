using ShopifyEasyShirtPrinting.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ShopifyEasyShirtPrinting.Models
{
    public class MyLineItem : EntityBase, INotifyPropertyChanged
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        [JsonPropertyName("OrderNumber")]
        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }

        [JsonPropertyName("Sku")]
        public string Sku { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        [JsonPropertyName("VariantId")]
        public long? VariantId { get; set; }
        [JsonPropertyName("VariantTitle")]
        public string VariantTitle { get; set; }
        [JsonPropertyName("LineItemId")]
        public long? LineItemId { get; set; }
        [JsonPropertyName("Quantity")]
        public int? Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        [JsonPropertyName("FulfillmentStatus")]
        public string FulfillmentStatus { get; set; }
        [JsonPropertyName("FinancialStatus")]
        public string FinancialStatus { get; set; }
        [JsonPropertyName("Customer")]
        public string Customer { get; set; }
        [JsonPropertyName("CustomerEmail")]
        public string CustomerEmail { get; set; }

        [JsonPropertyName("DateModified")]
        public DateTime? DateModified { get => _dateModified; set => SetProperty(ref _dateModified, value); }

        [JsonPropertyName("ProductImage")]
        public string ProductImage { get; set; }

        [JsonPropertyName("Notes")]
        public string Notes
        {
            get => _notes;
            set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(nameof(HasNotes));
            }
        }


        [JsonPropertyName("OrderId")]
        public long? OrderId { get; set; }

        private int _printedQuantity;
        private int? _binNumber;
        private string _status = "Pending";
        private string _notes;
        private DateTime? _dateModified;
        private int? _quantity;
        private string _orderNumber;
        private string _name;

        [JsonPropertyName("PrintedQuantity")]
        public int PrintedQuantity
        {
            get => _printedQuantity;
            set => SetProperty(ref _printedQuantity, value);
        }

        [JsonPropertyName("BinNumber")]
        public int? BinNumber
        {
            get => _binNumber;
            set => SetProperty(ref _binNumber, value);
        }

        [JsonPropertyName("Status")]
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        [JsonPropertyName("Shipping")]
        public string Shipping { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}