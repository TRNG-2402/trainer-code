using Microsoft.Extensions.Caching.Memory;
using Moq;
using ProductCatalog.Data;
using ProductCatalog.DTOs;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace WebStorefront.Tests.Services;

//https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=windows

// Follow the above guide to generate a test coverage report
// That you can view in your browser like a website, focus only on getting 20% branch coverage 
// in your service classes. Ignore everything else. 

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

    [Fact]
    public async Task DeleteCategoryAsync_InvalidId_ThrowsAndNeverTouchesRepoOrCache()
    {
        // Allocate/Arrange
        //Allocation is done, we already have our objects arranged for us

        //Act + Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.DeleteCategoryAsync(0) // When this method fires off with this input value, 
        );                                      // the OutOfRange exception is thrown

        // In addition to testing whether or not we threw the correct exception,
        // we can assert that certain methods fired x amount of times
        // in this case, none at all
        _repoMock.Verify(r => r.GetCategoryByIdAsync(It.IsAny<int>()), Times.Never);
        _repoMock.Verify(r => r.DeleteCategoryAsync(It.IsAny<Category>()), Times.Never);

        // Cache was also never touched
        _cacheMock.Verify(r => r.Remove(It.IsAny<Object>()), Times.Never);
    }

}