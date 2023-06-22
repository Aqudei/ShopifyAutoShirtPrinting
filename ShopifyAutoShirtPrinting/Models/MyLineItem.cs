using ShopifyEasyShirtPrinting.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public string OrderNumber { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public long? VariantId { get; set; }
        public string VariantTitle { get; set; }
        public long? LineItemId { get; set; }
        public int? Quantity { get; set; }
        public string FulfillmentStatus { get; set; }
        public string FinancialStatus { get; set; }
        public string Customer { get; set; }
        public string CustomerEmail { get; set; }

        public DateTime? DateModified { get => _dateModified; set => SetProperty(ref _dateModified, value); }

        public string ProductImage { get; set; }

        public string Notes
        {
            get => _notes; set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(nameof(HasNotes));
            }
        }
        public long? OrderId { get; set; }

        private int _printedQuantity;
        private int _binNumber;
        private string _status = "Pending";
        private string _notes;
        private DateTime? _dateModified;

        public int PrintedQuantity
        {
            get => _printedQuantity;
            set => SetProperty(ref _printedQuantity, value);
        }

        public int BinNumber
        {
            get => _binNumber;
            set => SetProperty(ref _binNumber, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        public string Shipping { get;  set; }

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