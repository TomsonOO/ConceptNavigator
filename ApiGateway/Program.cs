var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:4200")
			.AllowAnyMethod()
			.AllowAnyHeader();
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Configure routing to Quiz Service
app.Map("/api/quiz/{**catch-all}", async context =>
{
	var quizServiceUrl = "http://conceptnavigator-quiz-service";
	var path = context.Request.Path.Value?.Replace("/api/quiz", "");
	var query = context.Request.QueryString.Value;
	var url = $"{quizServiceUrl}{path}{query}";

	using var client = new HttpClient();
	var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), url);
	
	// Copy headers
	foreach (var header in context.Request.Headers)
	{
		request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
	}

	// Copy body for POST/PUT requests
	if (context.Request.Method == "POST" || context.Request.Method == "PUT")
	{
		var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
		request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
	}

	var response = await client.SendAsync(request);
	context.Response.StatusCode = (int)response.StatusCode;
	
	// Copy response headers
	foreach (var header in response.Headers)
	{
		context.Response.Headers[header.Key] = header.Value.ToArray();
	}

	// Copy response body
	var responseBody = await response.Content.ReadAsStringAsync();
	await context.Response.WriteAsync(responseBody);
});

// Configure routing to Session endpoints
app.Map("/api/session/{**catch-all}", async context =>
{
	var quizServiceUrl = "http://conceptnavigator-quiz-service";
	var path = context.Request.Path.Value?.Replace("/api/session", "/api/session");
	var query = context.Request.QueryString.Value;
	var url = $"{quizServiceUrl}{path}{query}";

	using var client = new HttpClient();
	var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), url);
	
	// Copy headers
	foreach (var header in context.Request.Headers)
	{
		request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
	}

	// Copy body for POST/PUT requests
	if (context.Request.Method == "POST" || context.Request.Method == "PUT")
	{
		var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
		request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
	}

	var response = await client.SendAsync(request);
	context.Response.StatusCode = (int)response.StatusCode;
	
	// Copy response headers
	foreach (var header in response.Headers)
	{
		context.Response.Headers[header.Key] = header.Value.ToArray();
	}

	// Copy response body
	var responseBody = await response.Content.ReadAsStringAsync();
	await context.Response.WriteAsync(responseBody);
});

app.UseAuthorization();
app.MapControllers();

app.Run();
