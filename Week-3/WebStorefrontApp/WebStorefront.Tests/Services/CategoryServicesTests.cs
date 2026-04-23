using Microsoft.Extensions.Caching.Memory;
using Moq;
using ProductCatalog.Data;
using ProductCatalog.DTOs;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace WebStorefront.Tests.Services;

public class CategoryServiceTests
{
    //Moq lets us create fake objects so we can avoid calling their real code
    //For us, we're going to create fake repo layer objects
    private readonly Mock<ICategoryRepo> _repoMock;

    // Don't forget the IMemoryCache! Since it is also a dependency
    private readonly Mock<IMemoryCache> _cacheMock;

    //We also want to create an actual CategoryService object
    private readonly CategoryService _sut; //system-under-test

    //We are going to create and set these objects in our constructor
    public CategoryServiceTests()
    {   
        // Creating mock objects for our dependencies
        _repoMock = new Mock<ICategoryRepo>(); 
        _cacheMock = new Mock<IMemoryCache>();

        // Using those mock objects to satisfy the CategoryService Constructor
        _sut = new CategoryService(_repoMock.Object, _cacheMock.Object);//when we create this category service object, we use our Mock
    }

}