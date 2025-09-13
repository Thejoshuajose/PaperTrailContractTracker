using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using MongoDB.Bson;
using MongoDB.Driver;
using Ookii.Dialogs.Wpf;
using PaperTrail.App.Services;
using PaperTrail.App.ViewModels;
using PaperTrail.App;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System;
using Windows.UI.Notifications;

namespace PaperTrail.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
