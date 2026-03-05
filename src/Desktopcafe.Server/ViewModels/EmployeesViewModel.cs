using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktopcafe.Core.Models;
using Desktopcafe.Data;
using Microsoft.EntityFrameworkCore;

namespace Desktopcafe.Server.ViewModels;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    public EmployeesViewModel(AppDbContext db)
    {
        _db = db;
    }

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [RelayCommand]
    private async Task AddAsync()
    {
        // Dialog-driven: view layer handles employee creation
        await LoadEmployeesAsync();
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedEmployee is null) return;

        try
        {
            _db.Employees.Update(SelectedEmployee);
            await _db.SaveChangesAsync();
            await LoadEmployeesAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedEmployee is null) return;

        try
        {
            SelectedEmployee.IsActive = false;
            _db.Employees.Update(SelectedEmployee);
            await _db.SaveChangesAsync();
            await LoadEmployeesAsync();
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }

    private async Task LoadEmployeesAsync()
    {
        try
        {
            var employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            Employees.Clear();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
        }
        catch (Exception)
        {
            // Logging handled elsewhere
        }
    }
}
