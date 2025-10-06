## Quick Start

1) Set environment variables (terminal):
```
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=translation_manager;Username=postgres;Password=postgres;"
export ASPNETCORE_URLS=http://localhost:5090
```

2) Run the API:
```
cd TranslationManager.API
dotnet restore
dotnet ef database update
dotnet run --urls="http://localhost:5090"
```

3) Serve the frontend (static):
```
cd ../frontend
python3 -m http.server 3000
```
Open http://localhost:3000 (API base: http://localhost:5090/api)


