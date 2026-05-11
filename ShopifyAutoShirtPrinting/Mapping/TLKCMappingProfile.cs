using AutoMapper;
using Common.Models;
using Common.Models.Harmonisation;
using ShopifyEasyShirtPrinting.Models.Harmonisation;
using ShopifyEasyShirtPrinting.Properties;
using ShopifyEasyShirtPrinting.ViewModels;
using ShopifyEasyShirtPrinting.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Mapping
{
    public class TLKCMappingProfile : Profile
    {
        public TLKCMappingProfile()
        {
            CreateMap<MyLineItem, MyLineItem>();
            CreateMap<MyLineItem, LineItemViewModel>()
            .ReverseMap();
            CreateMap<LineItemViewModel, LineItemViewModel>();
            CreateMap<MyLineItem, CrudDialogViewModel>()
            .ReverseMap();
            CreateMap<OrderInfo, OrderInfo>();
            CreateMap<Log, Log>();
            CreateMap<BinViewModel, Bin>()
            .ReverseMap();

            CreateMap<Settings, SettingsViewModel>()
            .ReverseMap();

            CreateMap<LoginDialogViewModel.SessionInfo, SessionVariables>();
            CreateMap<LoginDialogViewModel.LoginResultBody, SessionVariables>();
            CreateMap<Shipment, LabelPrintingDialogViewModel>();

            CreateMap<LabelPrintingDialogViewModel, CreateShipmentRequestBody>();
            CreateMap<Shipment, Models.Shipment>().ReverseMap();
            CreateMap<Common.Models.Harmonisation.HSN, Models.Harmonisation.HSN>().ReverseMap();

            CreateMap<Common.Models.Seo.SEOPage, Models.Seo.SEOPage>();
            CreateMap<Common.Models.Seo.SEOAudit, Models.Seo.SEOPage>();
            CreateMap<Common.Models.Seo.ScoreBreakdown, ViewModels.Dialogs.ScoreBreakdownViewModel>();
        }
    }
}
