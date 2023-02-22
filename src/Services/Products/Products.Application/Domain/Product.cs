using Products.Application.Shared;

namespace Products.Application.Domain
{
    public class Product : IAuditableEntity
    {
        // Required constructor
        public Product()
        {
            Id = 0;
            Name = default!;
            Price = 0;
            
        }
        
        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;
        }
        
        public int Id { get; set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }

        public void Update(string name, decimal price)
        {
            Name = name;
            Price = price;
        }

    }
}
