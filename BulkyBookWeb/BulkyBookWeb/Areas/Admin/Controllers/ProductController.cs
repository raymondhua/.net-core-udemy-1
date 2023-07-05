﻿using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.CloudStorage.Service;
using System.Threading.Tasks;

namespace BulkyBookWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IAzureStorage _azureStorage;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IAzureStorage azureStorage)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
        _azureStorage = azureStorage;
    }
    public IActionResult Index()
    {
        return View();
    }
    //GET
    public IActionResult Upsert(int? id)
    {
        ProductVM productVM = new()
        {
            Product = new(),
            CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }),
            CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            }),
        };

        if (id == null || id == 0)
        {
            //create product
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CoverTypeList"] = CoverTypeList;
            return View(productVM);
        }
        else
        {
            //update product
            productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
            return View(productVM);
        }
    }
    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM obj, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if(file != null)
            {
                if (obj.Product.ImageFileName != null)
                {
                    var imageExistsResult = Task.Run(async () => await _azureStorage.ImageExists(obj.Product.ImageUrl)).Result;
                    if (imageExistsResult != null && imageExistsResult)
                    {

                        Task.Run(async () => await(_azureStorage.DeleteAsync(obj.Product.ImageFileName)));
                    }
                }

                var uploadContents = Task.Run(async () => await _azureStorage.UploadAsync(file)).Result;
                obj.Product.ImageFileName = uploadContents.Blob.Name;
                obj.Product.ImageUrl = uploadContents.Blob.Uri;
            }
            if(obj.Product.Id == 0)
            {
                _unitOfWork.Product.Add(obj.Product);
            }
            else
            {
                _unitOfWork.Product.Update(obj.Product);
            }
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        return View(obj);
    }

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
        return Json(new { data = productList });
    }
    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        if (obj.ImageFileName != null)
        {
            var imageExistsResult = Task.Run(async () => await _azureStorage.ImageExists(obj.ImageFileName)).Result;
            if (imageExistsResult != null && imageExistsResult)
                Task.Run(async () => await _azureStorage.DeleteAsync(obj.ImageFileName));
        }
        _unitOfWork.Product.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Delete Successful" });
    }
    #endregion
}