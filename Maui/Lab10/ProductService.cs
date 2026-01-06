namespace Maui.Lab10;

public class ProductService : IProductService
{
    public List<Product> Products = [];
    
    public async Task<Product> AddProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public async Task<Product> GetProducts()
    {
        throw new NotImplementedException();
    }

    public async Task<Product> GetProductById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Product> UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public async Task<Product> DeleteProduct(Guid id)
    {
        throw new NotImplementedException();
    }
}