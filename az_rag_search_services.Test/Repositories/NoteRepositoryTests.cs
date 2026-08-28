using System.Net;
using az_rag_search_services.Application.Common.Interfaces;
using az_rag_search_services.Domain.Entities;
using az_rag_search_services.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace az_rag_search_services.Test.Repositories;

public class NoteRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<IAzureCosmosDbService> _mockAzureCosmosDbService;
    private readonly Mock<ILogger<NoteRepository>> _mockLogger;
    private readonly NoteRepository _noteRepository;

    public NoteRepositoryTests()
    {
        _mockContainer = new Mock<Container>();
        _mockAzureCosmosDbService = new Mock<IAzureCosmosDbService>();
        _mockLogger = new Mock<ILogger<NoteRepository>>();
        _mockAzureCosmosDbService
            .Setup(s => s.GetContainer())
            .Returns(_mockContainer.Object);
        _noteRepository = new NoteRepository(_mockAzureCosmosDbService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNote_WhenNoteExists()
    {
        var expectedNote = new Note("Test Content");
        var partitionKey = new PartitionKey(expectedNote.Id.ToString());
        var mockResponse = new Mock<ItemResponse<Note>>();
        mockResponse
            .Setup(r => r.Resource)
            .Returns(expectedNote);
        _mockContainer
            .Setup(c => c.ReadItemAsync<Note>(
                    It.Is<string>(s => s == expectedNote.Id.ToString()), 
                    It.Is<PartitionKey>(p => p.ToString() == partitionKey.ToString()),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockResponse.Object);

        var result = await _noteRepository.GetByIdAsync(expectedNote.Id.ToString());
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedNote.Id);
        _mockContainer.Verify(c => c.ReadItemAsync<Note>(
                expectedNote.Id.ToString(),
                partitionKey,
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    } 
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoteDoesNotExist()
    {
        var expectedNote = new Note("Test Content");
        var partitionKey = new PartitionKey(expectedNote.Id.ToString());
        _mockContainer
            .Setup(c => c.ReadItemAsync<Note>(
                    It.Is<string>(s => s == expectedNote.Id.ToString()), 
                    It.Is<PartitionKey>(p => p.ToString() == partitionKey.ToString()),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        var result = await _noteRepository.GetByIdAsync(expectedNote.Id.ToString());
        
        result.Should().BeNull();
        _mockLogger.Verify(                                                                                                                                                                                                                                           
            x => x.Log(                                                                                                                                                                                                                                               
                LogLevel.Error,                                                                                                                                                                                                                                       
                It.IsAny<EventId>(),                                                                                                                                                                                                                                  
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NoteRepository GetByIdAsync method")),                                                                                                                                                          
                It.IsAny<Exception>(),                                                                                                                                                                                                                                
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),                                                                                                                                                                                                  
            Times.Once); 
    } 
    
    [Fact]                                                                                                                                                                                                                                                                
    public async Task AddAsync_ShouldCallCreateItemAsync_WhenNoteIsValid()                                                                                                                                                                                                
    {                                                                                                                                                                                                                                                                     
        var note = new Note("Test content");                                                                                                                                                                                                                     
        var partitionKey = new PartitionKey(note.Id.ToString());                                                                                                                                                                                                          
                                                                                                                                                                                                                                                                          
        _mockContainer                                                                                                                                                                                                                                                    
            .Setup(c => c.CreateItemAsync(                                                                                                                                                                                                                                
                It.Is<Note>(n => n.Id == note.Id),                                                                                                                                                                                                                        
                partitionKey,                                                                                                                                                                                                                                             
                It.IsAny<ItemRequestOptions>(),                                                                                                                                                                                                                           
                It.IsAny<CancellationToken>()))                                                                                                                                                                                                                           
            .ReturnsAsync(new Mock<ItemResponse<Note>>().Object);                                                                                                                                                                                                         
        
        await _noteRepository.AddAsync(note);                                                                                                                                                                                                                             
                                                                                                                                                                                                                                                                          
        _mockContainer.Verify(c => c.CreateItemAsync(                                                                                                                                                                                                                     
            It.Is<Note>(n => n.Id == note.Id),                                                                                                                                                                                                                            
            partitionKey,                                                                                                                                                                                                                                                 
            It.IsAny<ItemRequestOptions>(),                                                                                                                                                                                                                               
            It.IsAny<CancellationToken>()),                                                                                                                                                                                                                               
            Times.Once);
        _mockLogger.Verify(                                                                                                                                                                                                                                               
            x => x.Log(                                                                                                                                                                                                                                                   
                LogLevel.Information,                                                                                                                                                                                                                                     
                It.IsAny<EventId>(),                                                                                                                                                                                                                                      
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NoteRepository AddAsync Started")),                                                                                                                                                                 
                It.IsAny<Exception>(),                                                                                                                                                                                                                                    
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),                                                                                                                                                                                                      
            Times.Once);                                                                                                                                                                                                                                                  
    }                                                                                                                                                                                                                                                                     
                                                                                                                                                                                                                                                                          
    [Fact]                                                                                                                                                                                                                                                                
    public async Task AddAsync_ShouldThrowException_WhenCosmosDbFails()                                                                                                                                                                                                   
    {                                                                                                                                                                                                                                                                     
        var note = new Note("Test content");                                                                                                                                                                                                               
        var exceptionMessage = "CosmosDB Connection error";    
                                                                                                                                                                                                                                                                          
        _mockContainer                                                                                                                                                                                                                                                    
            .Setup(c => c.CreateItemAsync(                                                                                                                                                                                                                                
                It.IsAny<Note>(),                                                                                                                                                                                                                                         
                It.IsAny<PartitionKey>(),                                                                                                                                                                                                                                 
                It.IsAny<ItemRequestOptions>(),                                                                                                                                                                                                                           
                It.IsAny<CancellationToken>()))                                                                                                                                                                                                                           
            .ThrowsAsync(new Exception(exceptionMessage));                                                                                                                                                                                                                
                                                                                                                                                                                                                                                                          
        var act = () => _noteRepository.AddAsync(note);                                                                                                                                                                                                                   
                                                                                                                                                                                                                                                                          
        await act.Should().ThrowAsync<Exception>()                                                                                                                                                                                                                        
            .WithMessage(exceptionMessage);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NoteRepository AddAsync method") &&
                                              v.ToString()!.Contains(exceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }       
}