#nullable enable

using AutoMapper;
using Common.Models.Seo;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class SEODetailViewModel : PageBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; private set; }

        public override string Title => "SEO Detail";


        private int _store;
        public int Store
        {
            get => _store;
            set => SetProperty(ref _store, value);
        }

        private string? _shopifyId;
        public string? ShopifyId
        {
            get => _shopifyId;
            set => SetProperty(ref _shopifyId, value);
        }

        private string? _handle;
        public string? Handle
        {
            get => _handle;
            set => SetProperty(ref _handle, value);
        }

        private string? _pageTitle;
        public string? PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string? _descriptionHtml;
        public string? DescriptionHtml
        {
            get => _descriptionHtml;
            set => SetProperty(ref _descriptionHtml, value);
        }

        private Uri? _url;
        public Uri? Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private string? _pageType;
        public string? PageType
        {
            get => _pageType;
            set => SetProperty(ref _pageType, value);
        }

        private string? _targetKeyword;
        public string? TargetKeyword
        {
            get => _targetKeyword;
            set => SetProperty(ref _targetKeyword, value);
        }

        private string? _seoTitle;
        public string? SEOTitle
        {
            get => _seoTitle;
            set => SetProperty(ref _seoTitle, value);
        }

        private string? _metaDescription;
        public string? MetaDescription
        {
            get => _metaDescription;
            set => SetProperty(ref _metaDescription, value);
        }

        private string? _imageUrl;
        public string? ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        private string? _imageAltText;
        public string? ImageAltText
        {
            get => _imageAltText;
            set => SetProperty(ref _imageAltText, value);
        }

        private string? _priority;
        public string? Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }

        private List<string>? _reasonFlagged;
        public List<string>? ReasonFlagged
        {
            get => _reasonFlagged;
            set => SetProperty(ref _reasonFlagged, value);
        }


        private int _pageId;
        public int PageId
        {
            get => _pageId;
            set => SetProperty(ref _pageId, value);
        }

        private float _score;
        public float Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }

        private string? _grade;
        public string? Grade
        {
            get => _grade;
            set => SetProperty(ref _grade, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private ScoreBreakdown[]? _breakdown;
        private readonly IMapper mapper;

        public ScoreBreakdown[]? Breakdown
        {
            get => _breakdown;
            set => SetProperty(ref _breakdown, value);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }


        public void OnDialogOpened(IDialogParameters parameters)
        {
            if(parameters.TryGetValue<Models.Seo.SEOPage>("page", out var page))
            {
                // Do something with the page
                mapper.Map(page, this);
            }
        }


        public SEODetailViewModel(IMapper mapper)
        {
            this.mapper = mapper;
        }
    }
}
