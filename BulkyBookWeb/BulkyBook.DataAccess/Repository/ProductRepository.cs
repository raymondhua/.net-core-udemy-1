using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.CloudStorage.Service;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        private readonly IAzureStorage _azureStorage;
        public ProductRepository(ApplicationDbContext db, IAzureStorage azureStorage) : base(db)
        {
            _db = db;
            _azureStorage = azureStorage;
        }
        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u=> u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price100 = obj.Price100;
                objFromDb.Description = obj.Description;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.Author = obj.Author;
                objFromDb.CoverTypeId = obj.CoverTypeId;
                if(obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
                if (obj.ImageFileName != null)
                {
                    objFromDb.ImageFileName = obj.ImageFileName;
                }
            }
        }
        //includeProp "Category,CoverType"
        public IEnumerable<Product> GetAllProducts(Expression<Func<Product, bool>>? filter = null, string? includeProperties = null)
        {
            IEnumerable<Product> query = new Repository<Product>(_db).GetAll(filter, includeProperties);

            foreach (var product in query)
            {
                var imageSRS = Task.Run(async () => await _azureStorage.GenerateSasToken()).Result;
                product.ImageUrl += imageSRS.Uri.Query;
            }
            return query.ToList();
        }

        public IEnumerable<Product> AppendSasTokensForImages(IEnumerable<Product> query)
        {
            foreach (var product in query)
            {
                product.ImageUrl += _azureStorage.GetSasToken();
            }
            return query.ToList();
        }

        public Product AppendSasTokenForImage(Product product)
        {
            product.ImageUrl += _azureStorage.GetSasToken();
            return product;
        }

    }
}
