dotnet build -c Release 

EXE="./bin/Release/net8.0/FrequencyCounter.exe"

"$EXE" analyze "../datasets/txt/tradition.txt" 