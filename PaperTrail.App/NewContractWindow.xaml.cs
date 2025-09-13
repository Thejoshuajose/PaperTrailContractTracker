using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace PaperTrail.App;

public partial class NewContractWindow : Window
{
    private class TemplateInfo
    {
        public string Category { get; }
        public string Title { get; }
        public TemplateInfo(string category, string title)
        {
            Category = category;
            Title = title;
        }
    }

    private readonly ICollectionView _view;

    public string? SelectedTitle { get; private set; }

    public NewContractWindow()
    {
        InitializeComponent();
        var templates = new List<TemplateInfo>
        {
            new("📑 Business & Commercial Contracts", "Partnership Agreement"),
            new("📑 Business & Commercial Contracts", "Shareholders Agreement"),
            new("📑 Business & Commercial Contracts", "Operating Agreement (LLC Agreement)"),
            new("📑 Business & Commercial Contracts", "Business Purchase Agreement (M&A)"),
            new("📑 Business & Commercial Contracts", "Non-Disclosure Agreement (NDA)"),
            new("📑 Business & Commercial Contracts", "Non-Compete Agreement"),
            new("📑 Business & Commercial Contracts", "Service Agreement / Consulting Agreement"),
            new("📑 Business & Commercial Contracts", "Supply Agreement"),
            new("📑 Business & Commercial Contracts", "Joint Venture Agreement"),
            new("📑 Business & Commercial Contracts", "Franchise Agreement"),
            new("⚖️ Employment & HR Contracts", "Employment Agreement"),
            new("⚖️ Employment & HR Contracts", "Independent Contractor Agreement"),
            new("⚖️ Employment & HR Contracts", "Severance Agreement"),
            new("⚖️ Employment & HR Contracts", "Employee Handbook Acknowledgement"),
            new("⚖️ Employment & HR Contracts", "Equity/Stock Option Agreement"),
            new("🏠 Real Estate & Property", "Lease Agreement (Residential or Commercial)"),
            new("🏠 Real Estate & Property", "Purchase & Sale Agreement (Real Estate)"),
            new("🏠 Real Estate & Property", "Easement Agreement"),
            new("🏠 Real Estate & Property", "Construction Contract"),
            new("🏠 Real Estate & Property", "Property Management Agreement"),
            new("💍 Family & Personal Contracts", "Prenuptial Agreement"),
            new("💍 Family & Personal Contracts", "Postnuptial Agreement"),
            new("💍 Family & Personal Contracts", "Separation/Divorce Settlement Agreement"),
            new("💍 Family & Personal Contracts", "Child Custody Agreement"),
            new("💍 Family & Personal Contracts", "Power of Attorney"),
            new("💍 Family & Personal Contracts", "Living Will / Advance Healthcare Directive"),
            new("📚 Intellectual Property & Creative", "Licensing Agreement"),
            new("📚 Intellectual Property & Creative", "Publishing Agreement"),
            new("📚 Intellectual Property & Creative", "Artist/Recording Contract"),
            new("📚 Intellectual Property & Creative", "Software Development Agreement"),
            new("📚 Intellectual Property & Creative", "SaaS Agreement"),
            new("💰 Finance & Lending", "Loan Agreement / Promissory Note"),
            new("💰 Finance & Lending", "Guaranty Agreement"),
            new("💰 Finance & Lending", "Security Agreement"),
            new("💰 Finance & Lending", "Investment Agreement"),
            new("🏛️ Government & Compliance", "Government Contract (Procurement)"),
            new("🏛️ Government & Compliance", "Compliance Agreement / Consent Decree"),
            new("🏛️ Government & Compliance", "Data Processing Agreement (DPA)"),
            new("📦 Miscellaneous / General Use", "Settlement Agreement"),
            new("📦 Miscellaneous / General Use", "Mediation/Arbitration Agreement"),
            new("📦 Miscellaneous / General Use", "General Release of Claims"),
            new("📦 Miscellaneous / General Use", "Waiver & Indemnity Agreement")
        };

        _view = CollectionViewSource.GetDefaultView(templates);
        _view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TemplateInfo.Category)));
        TemplateList.ItemsSource = _view;
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var text = SearchBox.Text;
        _view.Filter = item => string.IsNullOrWhiteSpace(text) ||
            ((TemplateInfo)item).Title.Contains(text, System.StringComparison.OrdinalIgnoreCase);
    }

    private void CreateSelected_Click(object sender, RoutedEventArgs e)
    {
        if (TemplateList.SelectedItem is TemplateInfo info)
        {
            SelectedTitle = info.Title;
            DialogResult = true;
        }
    }

    private void CreateCustom_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(CustomTitleBox.Text))
        {
            SelectedTitle = CustomTitleBox.Text;
            DialogResult = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
