using System.Net;
using Flippit.Web.App;
using Flippit.Web.BL;
using Flippit.Web.BL.Extensions;
using Flippit.Web.BL.Facades;
using Flippit.Web.BL.Installers;
using Flippit.Web.BL.Mappers;
using Flippit.Web.DAL;
using Flippit.Web.DAL.Repositories;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<UserState>();

// Configure LocalDbOptions from configuration
var localDbOptions = new LocalDbOptions();
builder.Configuration.GetSection("LocalDb").Bind(localDbOptions);
builder.Services.AddSingleton(localDbOptions);

// Get API base URL from configuration
var apiBaseUrl = builder.Configuration.GetValue<string>("Api:BaseUrl") ?? "https://flippit-api-gzescpcqgzdqfqh9.switzerlandnorth-01.azurewebsites.net";
//var apiBaseUrl = builder.Configuration.GetValue<string>("Api:BaseUrl") ?? "https://localhost:7175";


// Register HttpClient with base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

//builder.Services.AddScoped<CookieHandler>();

//void AddApiClient<TInterface, TImplementation>(string baseUrl)
//    where TInterface : class
//    where TImplementation : class, TInterface
//{
//    builder.Services.AddHttpClient<TInterface, TImplementation>(client =>
//    {
//        client.BaseAddress = new Uri(baseUrl);
//    })
//    .AddHttpMessageHandler<CookieHandler>(); 
//}

//AddApiClient<IApiApiClient, ApiApiClient>(apiBaseUrl);
//AddApiClient<IUsersApiClient, UsersApiClient>(apiBaseUrl);
//AddApiClient<ICardsApiClient, CardsApiClient>(apiBaseUrl);
//AddApiClient<ICollectionsApiClient, CollectionsApiClient>(apiBaseUrl);
//AddApiClient<ICompletedLessonsApiClient, CompletedLessonsApiClient>(apiBaseUrl);
//AddApiClient<IAuthApiClient, AuthApiClient>(apiBaseUrl);


// Register API clients
builder.Services.AddScoped<IApiApiClient, ApiApiClient>();
builder.Services.AddScoped<IUsersApiClient, UsersApiClient>();
builder.Services.AddScoped<ICardsApiClient, CardsApiClient>();
builder.Services.AddScoped<ICollectionsApiClient, CollectionsApiClient>();
builder.Services.AddScoped<ICompletedLessonsApiClient, CompletedLessonsApiClient>();
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();

// Register LocalDb and repositories
builder.Services.AddSingleton<LocalDb>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<CardRepository>();
builder.Services.AddSingleton<CollectionRepository>();
builder.Services.AddSingleton<CompletedLessonRepository>();

// Register mappers
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<CardMapper>();
builder.Services.AddSingleton<CollectionMapper>();
builder.Services.AddSingleton<CompletedLessonMapper>();
builder.Services.AddSingleton<ApiModelMapper>();

// Register facades
builder.Services.AddScoped<IUserFacade, UserFacade>();
builder.Services.AddScoped<ICardFacade, CardFacade>();
builder.Services.AddScoped<ICollectionFacade, CollectionFacade>();
builder.Services.AddScoped<ICompletedLessonFacade, CompletedLessonFacade>();


await builder.Build().RunAsync();
