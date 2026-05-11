#nullable enable

using AutoMapper;
using Common.Models.Seo;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.ViewModels.Dialogs
{
    public class ScoreBreakdownViewModel : PageBase, IDialogAware
    {
        private IMapper _mapper;


        private ObservableCollection<ScoreBreakdown> _scores = new();
        public ICollectionView ScoreBreakdowns { get; set; }

        public override string Title => "Score Breakdown";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // Handle any cleanup or state reset when the dialog is closed
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // Initialize or load data when the dialog is opened
            if (parameters.TryGetValue<ScoreBreakdown[]>("breakdown", out var breakdown))
            {
                _scores.Clear();
                _scores.AddRange(breakdown);
            }

        }

        public ScoreBreakdownViewModel(IMapper mapper)
        {
            _mapper = mapper;
            ScoreBreakdowns = CollectionViewSource.GetDefaultView(_scores);
        }
        
    }
}
