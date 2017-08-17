using GitSharpApi;
using GitSharpGui.Tools;
using GitSharpGui.ViewModels;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace GitSharpGui
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel = new MainWindowViewModel();
        private ILogger _logger = LogManager.CreateNullLogger();
        private MessageBoxFacade _messageBox;
        private GitProcess _gitProcess = new GitProcess();
        private GitApi _gitApi;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _messageBox = new MessageBoxFacade(_logger);
            _gitApi = new GitApi(_gitProcess);
        }

        private async void RepoTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                //using (var folderDialog = new FolderBrowserDialog())
                //{
                //    var dialogResult = folderDialog.ShowDialog();
                //    if (dialogResult != System.Windows.Forms.DialogResult.OK)
                //        throw new Exception("oop");
                //    _viewModel.RepoPath = folderDialog.SelectedPath;
                //}

                //_viewModel.RepoPath = folderDialog.SelectedPath;
                if (!Directory.Exists(_viewModel.RepoPath))
                {
                    _viewModel.Commits = new List<GitSharpApi.Models.Commit>();
                    return;
                }
                //_messageBox.ShowInfo($"Repo Path: '{_viewModel.RepoPath}'");
                _gitProcess.WorkingDirectory = _viewModel.RepoPath;
                _viewModel.Commits = (await _gitApi.GetCommitsAsync()).ToList();
            }
            catch (Exception ex)
            {
                _messageBox.ShowError(ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async void HashTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_viewModel.CurrentHash))
                {
                    _viewModel.OutputText = null;
                    return;
                }

                _viewModel.OutputText = await _gitApi.CatFileAsync(_viewModel.CurrentHash);
            }
            catch (Exception ex)
            {
                _messageBox.ShowError(ex);
            }
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                var grid = sender as System.Windows.Controls.DataGrid;
                var x = grid.CurrentItem as DataRowView;
                if (x == null)
                {
                    //_viewModel.CurrentHash = null;
                    return;
                }
                _viewModel.CurrentHash = x["Hash"] as string;
            }
            catch (Exception ex)
            {
                _messageBox.ShowError(ex);
            }
        }
    }
}