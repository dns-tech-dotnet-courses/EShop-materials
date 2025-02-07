using EShop.DAL;
using EShop.Domain;

namespace EShop.Application
{
    public class ProductHandler
    {
        private readonly IProductRepository _productRepository;

        public ProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IEnumerable<Product> Get()
        {
            var productsRepository = new ProductRepository();
            var products = productsRepository.Get();

            return products;
        }
    }
}