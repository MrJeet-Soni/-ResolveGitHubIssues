using ContosoShopEasy.Models;
using ContosoShopEasy.Data;

namespace ContosoShopEasy.Services
{
    public class ProductService
    {
        private const int MaxSearchTermLength = 100;
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            return _productRepository.GetProductById(id);
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            return _productRepository.GetProductsByCategory(categoryId);
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            var sanitizedSearchTerm = SanitizeSearchTerm(searchTerm);
            return sanitizedSearchTerm == null
                ? new List<Product>()
                : _productRepository.SearchProducts(sanitizedSearchTerm);
        }

        private static string? SanitizeSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length > MaxSearchTermLength)
                return null;

            var trimmedSearchTerm = searchTerm.Trim();
            if (trimmedSearchTerm.Any(char.IsControl) || ContainsSqlSyntax(trimmedSearchTerm))
                return null;

            return trimmedSearchTerm;
        }

        private static bool ContainsSqlSyntax(string searchTerm)
        {
            for (var index = 0; index < searchTerm.Length; index++)
            {
                var character = searchTerm[index];
                if (character is ';' or '"' or '`' or '\\')
                    return true;

                if (character == '-' && index + 1 < searchTerm.Length && searchTerm[index + 1] == '-')
                    return true;

                if ((character == '/' && index + 1 < searchTerm.Length && searchTerm[index + 1] == '*') ||
                    (character == '*' && index + 1 < searchTerm.Length && searchTerm[index + 1] == '/'))
                    return true;

                if (character == '\'' &&
                    (index == 0 || index == searchTerm.Length - 1 ||
                     !char.IsLetter(searchTerm[index - 1]) || !char.IsLetter(searchTerm[index + 1])))
                    return true;
            }

            return false;
        }

        public List<Product> GetTopRatedProducts(int count = 10)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToList();
        }

        public List<Product> GetFeaturedProducts(int count = 5)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.ReviewCount)
                .Take(count)
                .ToList();
        }

        public bool IsProductInStock(int productId, int quantity = 1)
        {
            var product = _productRepository.GetProductById(productId);
            return product != null && product.StockQuantity >= quantity;
        }

        public bool UpdateStock(int productId, int quantityChange)
        {
            var product = _productRepository.GetProductById(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                product.LastModified = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}