using Common.BGTasker;
using Common.Models;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.ViewModels
{
    internal class TasksViewModel : PageBase, INavigationAware
    {
        private readonly SessionVariables _sessionVariables;

        public override string Title => "Tasks";
        public ObservableCollection<IBGTask> Tasks { get; set; } = new();

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public TasksViewModel(SessionVariables sessionVariables)
        {
            _sessionVariables = sessionVariables;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Tasks.Clear();
            Tasks.AddRange(_sessionVariables.TaskQueue.ToList());
        }
    }
}
