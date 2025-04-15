using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Coffe_Grinder
{
    public partial class Adminhomepage : Page
    {
        private readonly Coffe_Grinder_DBEntities db = new Coffe_Grinder_DBEntities();

        public Adminhomepage()
        {
            InitializeComponent();
            Loaded += async (sender, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load Coffee Inventory
                CoffeeDataGrid.Visibility = Visibility.Collapsed;
                CoffeeLoadingIndicator.Visibility = Visibility.Visible;

                var coffeeInventory = await db.CoffeeInventories
                    .Include(c => c.CoffeeType)
                    .OrderBy(c => c.CoffeeName)
                    .ToListAsync();

                CoffeeDataGrid.ItemsSource = coffeeInventory;
                CoffeeLoadingIndicator.Visibility = Visibility.Collapsed;
                CoffeeDataGrid.Visibility = Visibility.Visible;

                // Load Recent Orders
                OrdersDataGrid.Visibility = Visibility.Collapsed;
                OrdersLoadingIndicator.Visibility = Visibility.Visible;

                var recentOrders = await db.Orders
                    .Include(o => o.OrderStatus)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync();

                OrdersDataGrid.ItemsSource = recentOrders;
                OrdersLoadingIndicator.Visibility = Visibility.Collapsed;
                OrdersDataGrid.Visibility = Visibility.Visible;

                // Load Low Stock Alerts (under 10kg)
                var lowStockItems = coffeeInventory
                    .Where(c => c.QuantityInStock < 10)
                    .OrderBy(c => c.QuantityInStock)
                    .ToList();

                if (!lowStockItems.Any())
                {
                    // Show message if no low stock items
                    LowStockList.ItemsSource = new[] {
                        new {
                            CoffeeName = "No low stock items",
                            QuantityInStock = 0,
                            CoffeeType = new { TypeName = "All items are sufficiently stocked" }
                        }
                    };
                }
                else
                {
                    LowStockList.ItemsSource = lowStockItems;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageInventory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new inventory());
        }

        private void ViewAllOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdersPage());
        }
    }
}