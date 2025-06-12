using QuizGenerationService.Application.Interfaces;
using QuizGenerationService.Application.Mapping;
using QuizGenerationService.Application.Services;
using QuizGenerationService.Application.Validation;
using QuizGenerationService.Domain.Interfaces;
using QuizGenerationService.Infrastructure.ExternalServices;
using QuizGenerationService.Infrastructure.Factories;
using QuizGenerationService.Infrastructure.PromptBuilding;
using QuizGenerationService.Infrastructure.SessionStorage;
using QuizGenerationService.Infrastructure.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<GeminiApiService>();
builder.Services.AddHttpClient<ExtendedExplanationApiService>();
builder.Services.AddHttpClient<KeywordExtractionApiService>();
builder.Services.AddHttpClient<ConceptMapApiService>();

builder.Services.AddScoped<PromptTemplateProvider>();
builder.Services.AddScoped<IPromptBuilder, PromptBuilder>();
builder.Services.AddScoped<AdaptivePromptBuilder>();

builder.Services.AddScoped<IQuizGenerationStrategy, BasicQuizGenerationStrategy>();
builder.Services.AddScoped<IQuizGenerationStrategy, SituationalQuizGenerationStrategy>();
builder.Services.AddScoped<QuizGenerationStrategyFactory>();

builder.Services.AddScoped<IQuizMappingService, QuizMappingService>();
builder.Services.AddScoped<QuizRequestValidator>();
builder.Services.AddScoped<ExplainMoreRequestValidator>();
builder.Services.AddScoped<GenerateMoreQuestionsValidator>();
builder.Services.AddScoped<GenerateConceptMapValidator>();
builder.Services.AddScoped<IQuizGenerationService, QuizGenerationService.Application.Services.QuizGenerationService>();

builder.Services.AddScoped<ISessionStorage, InMemorySessionStorage>();
builder.Services.AddScoped<ISessionManagementService, SessionManagementService>();

builder.Services.AddScoped<ExtendedExplanationApiService>();
builder.Services.AddScoped<IExtendedExplanationService, ExtendedExplanationService>();

builder.Services.AddScoped<KeywordExtractionApiService>();
builder.Services.AddScoped<IAdaptiveQuizService, AdaptiveQuizService>();

builder.Services.AddScoped<ConceptMapApiService>();
builder.Services.AddScoped<IConceptMapService, ConceptMapService>();

builder.Services.AddScoped<GeminiApiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
