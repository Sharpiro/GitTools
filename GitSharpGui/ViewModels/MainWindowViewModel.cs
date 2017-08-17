using GitSharpApi.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GitSharpGui.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _repoPath;
        private string _currentHash;
        private string _outputText;
        private List<Commit> _commits = new List<Commit>();

        public string RepoPath { get => _repoPath; set { _repoPath = value; OnPropertyChanged(); } }
        public string CurrentHash { get => _currentHash; set { _currentHash = value; OnPropertyChanged(); } }
        public string OutputText { get => _outputText; set { _outputText = value; OnPropertyChanged(); } }
        public List<Commit> Commits { get => _commits; set { _commits = value; OnPropertyChanged(); OnPropertyChanged(nameof(CommitsDataView)); } }
        public DataView CommitsDataView => Commits.ToDataTable().DefaultView;
    }
}