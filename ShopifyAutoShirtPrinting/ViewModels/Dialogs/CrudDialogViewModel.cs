using AutoMapper;
using Common.Api;
using Common.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    internal class CrudDialogViewModel : PageBase, IDialogAware
    {
        public override string Title => _title;

        private DelegateCommand<string> _dialogCommand;
        private MyLineItem _theLineItem;
        private string _orderNumber;
        private IMapper _mapper;
        private readonly ApiClient _api;
        private int _quantity = 1;
        private string notes;
        private string _name;
        private int _id;
        private string _title;
        private DelegateCommand _selectNoneCommand;
        public ObservableCollection<string> ShippingTypes { get; set; } = new();
        public string Shipping { get => _shipping; set => SetProperty(ref _shipping, value); }


        public DelegateCommand SelectNoneCommand
        {
            get { return _selectNoneCommand ??= new DelegateCommand(OnSelectNone); }
        }

        private void OnSelectNone()
        {
            SelectedVariant = null;
        }

        public DelegateCommand<string> DialogCommand
        {
            get { return _dialogCommand ??= new DelegateCommand<string>(OnDialogCommand); }
        }

        public string OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        public string Notes { get => notes; set => SetProperty(ref notes, value); }
        public int Id { get => _id; set => SetProperty(ref _id, value); }
        private string _searchText;
        private ICollectionView _variantsCollectionView;
        private Variant _selectedVariant;
        private string _sku;
        private string _shipping;

        public Variant SelectedVariant { get => _selectedVariant; set => SetProperty(ref _selectedVariant, value); }

        private ObservableCollection<Variant> _variants { get; set; } = new();
        public ICollectionView VariantsCollectionView { get => _variantsCollectionView; set => SetProperty(ref _variantsCollectionView, value); }

        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }

        public int Item { get; set; }
        public string Sku { get => _sku; set => SetProperty(ref _sku, value); }

        private void OnDialogCommand(string command)
        {
            _theLineItem.OrderNumber = OrderNumber;
            _theLineItem.Name = Name;
            _theLineItem.Quantity = Quantity;
            _theLineItem.Notes = Notes;
            _theLineItem.Sku = Sku;
            _theLineItem.Shipping = String.IsNullOrWhiteSpace(Shipping) ? _theLineItem.Shipping : Shipping;

            var prams = new DialogParameters { { "MyLineItem", _theLineItem } };

            RequestClose.Invoke(new DialogResult(ButtonResult.OK, prams));
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        public CrudDialogViewModel(IMapper mapper, ApiClient apiClient)
        {
            _mapper = mapper;
            _api = apiClient;

            VariantsCollectionView = CollectionViewSource.GetDefaultView(_variants);
            PropertyChanged += CrudDialogViewModel_PropertyChanged;
        }

        private async void CrudDialogViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SearchText):
                    {
                        if (!string.IsNullOrWhiteSpace(SearchText) && SearchText.Length >= 3)
                        {
                            var variants = await _api.SearchVariants(SearchText);
                            if (variants != null && variants.Any())
                            {
                                await _dispatcher.InvokeAsync(_variants.Clear);
                                foreach (var variant in variants)
                                {
                                    await _dispatcher.InvokeAsync(() =>
                                    {
                                        _variants.Add(variant);
                                    });
                                }
                            }
                        }
                        break;
                    }
                case nameof(SelectedVariant):
                    {
                        if (SelectedVariant != null)
                        {
                            _theLineItem.VariantId = SelectedVariant.ShopifyId;
                            _theLineItem.VariantTitle = SelectedVariant.Title;

                            Sku = SelectedVariant.Sku;
                            Name = $"{SelectedVariant.Product?.Title} - {SelectedVariant.Title}";
                        }
                        else
                        {
                            Name = "";
                            Sku = "";

                            _theLineItem.VariantTitle = "";
                            _theLineItem.VariantId = 0;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {

            var shippingTypes = await _api.ListShippingTypeAsync();

            await _dispatcher.InvokeAsync(() =>
            {
                ShippingTypes.Clear();
                if (shippingTypes != null && shippingTypes.Any())
                {
                    ShippingTypes.AddRange(shippingTypes);
                    Shipping = ShippingTypes.First();
                }
            });

            if (parameters != null && parameters.TryGetValue<MyLineItem>("MyLineItem", out var myLineItem))
            {
                _title = "Edit Item";
                _theLineItem = myLineItem;

                await _dispatcher.InvokeAsync(() => _mapper.Map(_theLineItem, this));
            }
            else
            {
                _title = "New Item";

                _theLineItem = new MyLineItem
                {
                    Status = "Pending"
                };
            }
        }
    }
}
