namespace Maui.Lab10;

public interface IProductService
{
    Task<Product> AddProduct(Product product); 
    Task<Product> GetProducts();
    Task<Product> GetProductById(Guid id); 
    Task<Product> UpdateProduct(Product product); 
    Task<Product> DeleteProduct(Guid id);
}